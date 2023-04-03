using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

using EXGuard.Core;
using EXGuard.Core.JIT;
using EXGuard.Core.Properties;

using MethodAttributes = dnlib.DotNet.MethodAttributes;

namespace EXGuard.Internal
{
    public class InitializePhase
    {
        Dictionary<IMemberRef, IMemberRef> refRepl;

        public ModuleDefMD DFModule
        {
            get;
            private set;
        }

        public HashSet<MethodDef> Methods
        {
            get;
            set;
        }

        public Virtualizer VR
        {
            get;
            private set;
        }

        public string RT_OUT_Directory
        {
            get;
            set;
        }

        public string RTName
        {
            get;
            set;
        }

        public string SNK_File
        {
            get;
            set;
        }

        public string SNK_Password
        {
            get;
            set;
        }

        public InitializePhase(ModuleDefMD module)
        {
            DFModule = module;
            Methods = new HashSet<MethodDef>();
            refRepl = new Dictionary<IMemberRef, IMemberRef>();
        }

        public void Initialize()
        {
            Methods = new HashSet<MethodDef>(Methods.Distinct().ToList());
            VR = new Virtualizer(DFModule, RTName);

            var oldType = DFModule.GlobalType;
            var newType = new TypeDefUser(oldType.Name);

            oldType.Name = "EXGuarder";
            oldType.BaseType = DFModule.CorLibTypes.GetTypeRef("System", "Object");

            DFModule.Types.Insert(0, newType);

            var old_cctor = oldType.FindOrCreateStaticConstructor();
            var cctor = newType.FindOrCreateStaticConstructor();

            old_cctor.Name = "Startup";
            old_cctor.IsRuntimeSpecialName = false;
            old_cctor.IsSpecialName = false;
            old_cctor.Access = MethodAttributes.Assembly;

            cctor.Body = new CilBody(true, new List<Instruction> {
                Instruction.Create(OpCodes.Jmp, old_cctor),
                Instruction.Create(OpCodes.Ret)
            }, new List<ExceptionHandler>(), new List<Local>());

            #region Import Runtime Entry Initialize
            ////////////////////////////////////////////////////////////
            VR.Runtime.RTMutator.ImportEntryInitialize(DFModule);
            ////////////////////////////////////////////////////////////
            #endregion

            for (int i = 0; i < oldType.Methods.Count; i++)
            {
                var nativeMethod = oldType.Methods[i];
                if (nativeMethod.IsNative)
                {
                    var methodStub = new MethodDefUser(nativeMethod.Name, nativeMethod.MethodSig.Clone());

                    methodStub.Attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                    methodStub.Body = new CilBody();
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Jmp, nativeMethod));
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                    newType.Methods[i] = methodStub;
                    newType.Methods.Add(nativeMethod);

                    refRepl[nativeMethod] = nativeMethod;
                }
            }

            Methods.Remove(cctor);
            Methods.Add(old_cctor);

            foreach (var entry in Methods)
                VR.AddMethod(entry);

            Utils.ExecuteModuleWriterOptions = new ModuleWriterOptions((ModuleDefMD)DFModule)
            {
                Logger = DummyLogger.NoThrowInstance,

                PdbOptions = PdbWriterOptions.None,
                WritePdb = false
            };

            //Utils.ExecuteModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;

            if (!string.IsNullOrEmpty(SNK_File))
            {
                if (File.Exists(SNK_File))
                {
                    StrongNameKey signatureKey = Utils.LoadSNKey(SNK_File, SNK_Password);
                    Utils.ExecuteModuleWriterOptions.InitializeStrongNameSigning(DFModule, signatureKey);
                }
            }

            VR.JIT(DFModule, Utils.ExecuteModuleWriterOptions, out var jitCtx);

            Utils.ExecuteModuleWriterOptions.WriterEvent += delegate (object sender, ModuleWriterEventArgs e)
            {
                var _writer = (ModuleWriterBase)sender;

                if (e.Event == ModuleWriterEvent.MDBeginWriteMethodBodies)
                {
                    VR.ProcessMethods(_writer);

                    foreach (var repl in refRepl)
                        VR.Runtime.Descriptor.Data.ReplaceReference(repl.Key, repl.Value);

                    VR.CommitModule(_writer.Metadata);

                    #region Configure JIT
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////
                    foreach (var jitMethod in jitCtx.Targets)
                    {
                        JITContext.RealBodies.Add(jitMethod.Body);

                        jitMethod.Body = JITWriter.NopBody(_writer.Module);
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion
                }
            };
        }
        
        public void GetProtectedFile(out byte[] jitedEXEC)
        {
            #region Extract Virted EXEC
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            MemoryStream output = new MemoryStream();
            DFModule.Write(output, Utils.ExecuteModuleWriterOptions);
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            jitedEXEC = output.ToArray();
        }

        public void SaveRuntime()
        {
            var rt = new MemoryStream();
            VR.Runtime.RTModule.Write(rt, VR.Runtime.RTModuleWriterOptions);

            var wow64Value = IntPtr.Zero;
            NativeMethods.Wow64DisableWow64FsRedirection(ref wow64Value);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            #region Check New Runtime Name
            ////////////////////////////////////////////////
            if (Path.GetExtension(RTName) != ".dll")
                RTName += ".dll";
            ////////////////////////////////////////////////
            #endregion

            var WriteDirectory = Path.Combine(RT_OUT_Directory, RTName);

            var TempDir = Environment.GetEnvironmentVariable("TEMP");

            var one = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(VR.Runtime.RTSearch.VMData.Name));
            var two = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(VR.Runtime.RTSearch.JITRuntime.Name));
            Buffer.BlockCopy(one, 0, one, 0, 16);
            Buffer.BlockCopy(two, 0, two, 0, 16);
            var TempFile = Path.Combine(Path.Combine(Path.GetTempPath(), new Guid(one).ToString().Replace("-", string.Empty)), new Guid(two).ToString().Substring(0, 7));

            var VMPEXEName = "22c36ed9";
            var VMPPROJName = "6c63ba12";
            var VMPOUTDLLName = "3b87814f";

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var ThreadA = new Thread(() =>
            {
                if (!Directory.Exists(TempDir))
                    Directory.CreateDirectory(TempDir);

                if (!Directory.Exists(Path.GetDirectoryName(TempFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(TempFile));

                if (File.Exists(Path.Combine(Path.GetDirectoryName(TempFile), VMPPROJName)))
                    File.Delete(Path.Combine(Path.GetDirectoryName(TempFile), VMPPROJName));

                if (File.Exists(Path.Combine(Path.GetDirectoryName(TempFile), VMPEXEName)))
                    File.Delete(Path.Combine(Path.GetDirectoryName(TempFile), VMPEXEName));

                if (File.Exists(Path.Combine(Path.GetDirectoryName(TempFile), VMPOUTDLLName)))
                    File.Delete(Path.Combine(Path.GetDirectoryName(TempFile), VMPOUTDLLName));

                if (File.Exists(WriteDirectory))
                    File.Delete(WriteDirectory);

                File.WriteAllBytes(TempFile, rt.ToArray());

                Thread.Sleep(100);

                rt.Flush();
                rt.Close();

                Thread.Sleep(300);
            });

            ThreadA.Start();
            ThreadA.Join();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var ThreadB = new Thread(() =>
            {
                var PROJ = new StringBuilder();
                PROJ.Append(ResourcesEX.VMProj);
                PROJ.Replace("dlldir", TempFile);
                PROJ.Replace("dlloutdir", Path.Combine(Path.GetDirectoryName(TempFile), VMPOUTDLLName));

                File.WriteAllText(Path.Combine(Path.GetDirectoryName(TempFile), VMPPROJName), PROJ.ToString());
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(TempFile), VMPEXEName), Resources.VMProtect_Con);

                Thread.Sleep(300);
            });

            ThreadB.Start();
            ThreadB.Join();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "\"" + Path.Combine(Path.GetDirectoryName(TempFile), VMPEXEName) + "\"",
                Arguments = "\"" + Path.Combine(Path.GetDirectoryName(TempFile), VMPPROJName) + "\""
            };

            process.Start();
            process.WaitForExit();

            Thread.Sleep(1000);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var readProtectedLibBuffer = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(TempFile), VMPOUTDLLName));
            File.WriteAllBytes(WriteDirectory, readProtectedLibBuffer);

            Thread.Sleep(1000);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (Directory.Exists(Path.GetDirectoryName(TempFile)))
                Directory.Delete(Path.GetDirectoryName(TempFile), true);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            NativeMethods.Wow64RevertWow64FsRedirection(wow64Value);
        }
    }
}