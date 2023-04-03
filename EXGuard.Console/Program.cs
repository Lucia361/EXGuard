using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using dnlib.DotNet;

using KeyAuth;
using Ionic.Zip;

using EXGuard.Core;
using EXGuard.Internal;
using EXGuard.Core.Properties;
using EXGuard.Core.RTProtections;
using EXGuard.Core.EXECProtections;
using EXGuard.Core.RTProtections.KroksCFlow;
using EXGuard.Core.EXECProtections.CEXCFlow;
using EXGuard.Core.EXECProtections._Mutation;

using EXGuard.Console.Services;
using EXGuard.Console.Properties;

namespace EXGuard.Console
{
    public static class Program
    {
        public static api KeyAuthApp = new api
        (
            name: "EXGuard",
            ownerid: "LQ7bYN8ZZF",
            secret: "bf64a128486912957bec38f420681666ecc10b0bbfb43630535a792e515af10e",
            version: "1.0"
        );

        static string CrashLog_Directory
        {
            get;
            set;
        }

        static string CrashLog_PHP_Directory
        {
            get;
            set;
        }

        #region Methods of Events
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            CrashLogger((Exception)e.ExceptionObject);
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            CrashLogger(e.Exception);
        }

        private static void CrashLogger(Exception ex)
        {
            var errtext = new StringBuilder();

            errtext.AppendLine(ex.Message);

            errtext.AppendLine();

            errtext.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            errtext.AppendLine(ex.StackTrace);
            errtext.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

            File.WriteAllText(Path.Combine(CrashLog_Directory, "conerr.log"), errtext.ToString());

            File.WriteAllText(Path.Combine(CrashLog_PHP_Directory, "conerr.log"), errtext.ToString());

            Environment.Exit(0);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        [STAThread]
        static void Main(string[] args) // args[0] = zip file | args[1] = obfuscated out rar file | args[2] = crash log location
        {
            var bytes = File.ReadAllBytes(args[0]);

            string Vanilla_File_Dir = string.Empty;
            string Protected_File_Dir = string.Empty;

            string Vanilla_File_Location = string.Empty;
            string Protected_File_Location = string.Empty;

            CrashLog_PHP_Directory = args[2];

            KeyAuthApp.init();

            var ThreadA = new Thread(() =>
            {
                using (var archiveStream = new MemoryStream(ArchiveEncryptionAlgorithm.Decrypt(bytes))) // Decrypt and Read Archive
                {
                    using (var archive = ZipFile.Read(archiveStream))
                    {
                        var EXGuard_Project_Entry = archive.GetEntry("EXGuard_Project.exproj");
                        var EXGuard_Project_Stream = EXGuard_Project_Entry.OpenReader();
                        var EXGuard_Project_MemoryStream = EXGuard_Project_Stream.ToMemoryStream();

                        #region Read EXGuard Project
                        /////////////////////////////////////////////////
                        XMLUtils.Read(EXGuard_Project_MemoryStream);
                        /////////////////////////////////////////////////
                        #endregion

                        #region Read Project Settings
                        //////////////////////////////////////////////////////////////////////////////////////////////////
                        var mdFullNames = new HashSet<string>();
                        var mdTokens = new HashSet<int>();

                        for (int i = 0; i < XMLUtils.Methods_FullNames.Count; i++)
                            mdFullNames.Add(XMLUtils.Methods_FullNames[i]);

                        for (int i = 0; i < XMLUtils.Methods_Tokens.Count; i++)
                            mdTokens.Add(XMLUtils.Methods_Tokens[i]);

                        string BYR_Username = XMLUtils.UserName;
                        string BYR_Password = XMLUtils.Password;

                        string BYR_InputFileName = XMLUtils.InputFileName;
                        string BYR_RuntimeName = XMLUtils.RuntimeName;
                        //////////////////////////////////////////////////////////////////////////////////////////////////
                        #endregion

                        KeyAuthApp.login(BYR_Username, BYR_Password);

                        if (KeyAuthApp.response.success)
                        {
                            BYR_Username = BYR_Username.Replace(" ", string.Empty);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Application Language
                            //////////////////////////////////////////////////////////////////////////////////////////
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                            //////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region System Events
                            ////////////////////////////////////////////////////////////////////////////////////////////////
                            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler; // Crash Dump
                            Application.ThreadException += GlobalThreadExceptionHandler; // Crash Dump
                            ////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            var EXGuard_InputFile_Stream = archive.GetEntry(BYR_InputFileName).OpenReader();
                            var EXGuard_InputFile_Buffer = EXGuard_InputFile_Stream.ToMemoryStream().ToArray();

                            string Current_Dir = Path.GetDirectoryName(typeof(Program).Assembly.Location);

                            string Users_Log_Dir = string.Empty;
                            string BYR_User_Dir = string.Empty;
                            string BYR_File_Dir = string.Empty;
                            string BYR_File_Info_Dir = string.Empty;
                            string BYR_SNK_File_Dir = string.Empty;

                            string SNK_File_Location = string.Empty;

                            string Vanilla_File_Info_File_Location = string.Empty;
                            string Protected_File_Info_File_Location = string.Empty;

                            #region Create Users Special Directory
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            Users_Log_Dir = Path.Combine(Current_Dir, "Users Log");
                            BYR_User_Dir = Path.Combine(Users_Log_Dir, BYR_Username);
                            BYR_File_Dir = Path.Combine(BYR_User_Dir,
                                Path.GetFileNameWithoutExtension(BYR_InputFileName).Replace(" ", string.Empty) +
                                "_" + Utils.File_Create_Murmur2(EXGuard_InputFile_Buffer));
                            BYR_File_Info_Dir = Path.Combine(BYR_File_Dir, "File_About");
                            BYR_SNK_File_Dir = Path.Combine(BYR_File_Dir, "SNK_File");
                            Vanilla_File_Dir = Path.Combine(BYR_File_Dir, "Vanilla_File");
                            Protected_File_Dir = Path.Combine(BYR_File_Dir, "Protected_File");
                            CrashLog_Directory = Path.Combine(BYR_File_Dir, "Crash_Log");

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            if (!Directory.Exists(Users_Log_Dir))
                                Directory.CreateDirectory(Users_Log_Dir);

                            if (!Directory.Exists(BYR_User_Dir))
                                Directory.CreateDirectory(BYR_User_Dir);

                            if (!Directory.Exists(BYR_File_Dir))
                                Directory.CreateDirectory(BYR_File_Dir);

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            if (!Directory.Exists(BYR_File_Info_Dir))
                                Directory.CreateDirectory(BYR_File_Info_Dir);
                            else
                            {
                                Directory.Delete(BYR_File_Info_Dir, true);
                                Directory.CreateDirectory(BYR_File_Info_Dir);
                            }

                            if (!Directory.Exists(BYR_SNK_File_Dir))
                                Directory.CreateDirectory(BYR_SNK_File_Dir);
                            else
                            {
                                Directory.Delete(BYR_SNK_File_Dir, true);
                                Directory.CreateDirectory(BYR_SNK_File_Dir);
                            }

                            if (!Directory.Exists(Vanilla_File_Dir))
                                Directory.CreateDirectory(Vanilla_File_Dir);
                            else
                            {
                                Directory.Delete(Vanilla_File_Dir, true);
                                Directory.CreateDirectory(Vanilla_File_Dir);
                            }

                            if (!Directory.Exists(Protected_File_Dir))
                                Directory.CreateDirectory(Protected_File_Dir);
                            else
                            {
                                Directory.Delete(Protected_File_Dir, true);
                                Directory.CreateDirectory(Protected_File_Dir);
                            }

                            if (!Directory.Exists(CrashLog_Directory))
                                Directory.CreateDirectory(CrashLog_Directory);
                            else
                            {
                                Directory.Delete(CrashLog_Directory, true);
                                Directory.CreateDirectory(CrashLog_Directory);
                            }

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            if (XMLUtils.SNKFile_Bytes.Length != 0)
                                SNK_File_Location = Path.Combine(BYR_SNK_File_Dir, XMLUtils.SNKFile_Name);

                            Vanilla_File_Location = Path.Combine(Vanilla_File_Dir, Path.GetFileName(BYR_InputFileName));
                            Protected_File_Location = Path.Combine(Protected_File_Dir, Path.GetFileName(BYR_InputFileName));

                            Vanilla_File_Info_File_Location = Path.Combine(BYR_File_Info_Dir, Path.GetFileName("Vanilla_File.txt"));
                            Protected_File_Info_File_Location = Path.Combine(BYR_File_Info_Dir, Path.GetFileName("Protected_File.txt"));
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            Thread.Sleep(200);

                            EXGuard_InputFile_Stream.Close();
                            EXGuard_InputFile_Stream.Dispose();

                            Thread.Sleep(200);

                            #region Zip Extract To Users->Vanilla_File Directory (Without Project)
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            for (int c = 0; c < archive.Entries.Count; c++)
                                if (archive.Entries.ToArray()[c] != EXGuard_Project_Entry)
                                    archive.Entries.ToArray()[c].Extract(Vanilla_File_Dir, ExtractExistingFileAction.OverwriteSilently);
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region SNK File Write To SNK_File Directory
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.SNKFile_Bytes.Length != 0)
                            {
                                File.WriteAllBytes(SNK_File_Location, XMLUtils.SNKFile_Bytes);
                            }
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            Thread.Sleep(200);

                            var module = ModuleDefMD.Load(Vanilla_File_Location);
                            var methods = new HashSet<MethodDef>();

                            module.AssemblyReferencesAdder();

                            #region Find Costura and Extract
                            ///////////////////////////////////////////////////////////////////
                            CosturaFodyDecompressor.ExtractDLLs(module, Vanilla_File_Dir);
                            ///////////////////////////////////////////////////////////////////
                            #endregion

                            //foreach (var item in module.Types)
                            //{
                            //    foreach (var item_ in item.Methods)
                            //    {
                            //        methods.Add(item_);
                            //    }
                            //}

                            foreach (var mdToken in mdTokens)
                                methods.Add(module.ResolveMethod(module.ResolveToken(mdToken).Rid));

                            #region Protections
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Virtualize All Reference Proxies
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.VirtualizeAllReferenceProxies)
                            {
                                RPNormal.Execute(module);

                                foreach (var refmd in RPNormal.ProxyMethods)
                                    methods.Add(refmd);
                            }
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti Debug and Anti VM
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiDebug && XMLUtils.AntiVM)
                                ResourcesEX.VMProj = ResourcesEX.VMProj.Replace("4552", "138696");
                            else if (XMLUtils.AntiDebug && !XMLUtils.AntiVM)
                                ResourcesEX.VMProj = ResourcesEX.VMProj.Replace("4552", "136648");
                            else if (!XMLUtils.AntiDebug && XMLUtils.AntiVM)
                                ResourcesEX.VMProj = ResourcesEX.VMProj.Replace("4552", "6600");

                            if (XMLUtils.AntiDebug)
                                foreach (var antdbg in AntiDebug_Inject.Execute(module))
                                    methods.Add(antdbg);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti Dump
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiDump)
                                foreach (var antdmp in AntiDump_Inject.Execute(module))
                                    methods.Add(antdmp);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti ILDasm
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiILDasm)
                                AntiILDasm_Inject.Execute(module);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti De4dot
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiDe4dot)
                                AntiDe4dot_Inject.Execute(module);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti Dnspy
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiDnspy)
                                foreach (var antdnspy in AntiDnspy_Inject.Execute(module))
                                    methods.Add(antdnspy);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Anti Web Debuggers
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.AntiWebDebuggers)
                                foreach (var antwebdbg in AntiWebDebuggers_Inject.Execute(module))
                                    methods.Add(antwebdbg);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Resource Protection
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.ResourceCompressAndEncrypt)
                                foreach (var resprot in ResourceProt_Inject.Execute(module))
                                    methods.Add(resprot);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Virtualize All Strings
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.VirtualizeAllStrings)
                                foreach (var hcst in module.Types.ToArray())
                                    if (hcst != module.GlobalType)
                                        foreach (var hcsm in hcst.Methods.ToArray())
                                            if (!methods.Contains(hcsm))
                                                new HideCallString(module).Execute(hcst, hcsm);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Virtualize All Numbers
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.VirtualizeAllNumbers)
                                foreach (var hcst in module.Types.ToArray())
                                    if (hcst != module.GlobalType)
                                        foreach (var hcsm in hcst.Methods.ToArray())
                                            if (!methods.Contains(hcsm))
                                                new HideCallNumber(module).Execute(hcst, hcsm);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Code Flow
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.CodeFlow)
                                foreach (var mtt in module.Types.ToArray())
                                    if (mtt != module.GlobalType)
                                        foreach (var mtm in mtt.Methods.ToArray())
                                            if (!RPNormal.ProxyMethods.Contains(mtm) && !methods.Contains(mtm))
                                                CEXControlFlow.Execute(mtm, 1);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Code Mutation
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (XMLUtils.CodeMutation)
                                foreach (var mtt in module.Types.ToArray())
                                    if (mtt != module.GlobalType)
                                        foreach (var mtm in mtt.Methods.ToArray())
                                            if (!RPNormal.ProxyMethods.Contains(mtm))
                                                MutationProt.Execute(module, mtm);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Virtualize
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            new EXGuardTask().Exceute(module, methods, Protected_File_Location, BYR_RuntimeName, SNK_File_Location, XMLUtils.SNK_Password);
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Vanilla and Protected File About Write
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (File.Exists(Protected_File_Location))
                            {
                                var vstrBuilder = new StringBuilder();
                                var pstrBuilder = new StringBuilder();

                                var vfInfo = new FileInfo(Vanilla_File_Location);
                                var pfInfo = new FileInfo(Protected_File_Location);

                                vstrBuilder.AppendLine(string.Format("{0}: {1}", "File Name", vfInfo.Name));
                                pstrBuilder.AppendLine(string.Format("{0}: {1}", "File Name", pfInfo.Name));

                                vstrBuilder.AppendLine(string.Format("{0}: {1}", "File Size", Utils.BytesSizeRead(vfInfo.Length)));
                                pstrBuilder.AppendLine(string.Format("{0}: {1}", "File Size", Utils.BytesSizeRead(pfInfo.Length)));

                                vstrBuilder.AppendLine(string.Format("{0}: {1}", "Creation Time (UTC)", vfInfo.CreationTimeUtc));
                                pstrBuilder.AppendLine(string.Format("{0}: {1}", "Creation Time (UTC)", pfInfo.CreationTimeUtc));

                                vstrBuilder.AppendLine(string.Format("{0}: {1}", "File MD5 Hash", Utils.File_Create_MD5(Vanilla_File_Location)));
                                pstrBuilder.AppendLine(string.Format("{0}: {1}", "File MD5 Hash", Utils.File_Create_MD5(Protected_File_Location)));

                                vstrBuilder.AppendLine(string.Format("{0}: {1}", "File Murmur2 Hash", Utils.File_Create_Murmur2(File.ReadAllBytes(Vanilla_File_Location))));
                                pstrBuilder.AppendLine(string.Format("{0}: {1}", "File Murmur2 Hash", Utils.File_Create_Murmur2(File.ReadAllBytes(Protected_File_Location))));

                                vstrBuilder.AppendLine();
                                pstrBuilder.AppendLine();

                                for (int f = 0; f < mdFullNames.Count; f++)
                                {
                                    vstrBuilder.AppendLine($"|{ mdFullNames.ToList()[f] } | { mdTokens.ToList()[f] }|");
                                    pstrBuilder.AppendLine($"|{ mdFullNames.ToList()[f] } | { mdTokens.ToList()[f] }|");
                                }

                                vfInfo = null;
                                pfInfo = null;

                                if (File.Exists(Vanilla_File_Info_File_Location))
                                    File.Delete(Vanilla_File_Info_File_Location);

                                if (File.Exists(Protected_File_Info_File_Location))
                                    File.Delete(Protected_File_Info_File_Location);

                                File.WriteAllText(Vanilla_File_Info_File_Location, vstrBuilder.ToString());
                                File.WriteAllText(Protected_File_Info_File_Location, pstrBuilder.ToString());
                            }
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Save Computer Info
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            var compInfo = new StringBuilder();
                            compInfo.AppendLine($"OS: { XMLUtils.ComputerOS }");
                            compInfo.AppendLine($"CPU: { XMLUtils.ComputerCPUName }");
                            compInfo.AppendLine($"Username: { XMLUtils.ComputerUserName }");
                            compInfo.AppendLine($"RAM: { XMLUtils.ComputerRamSize }");
                            compInfo.AppendLine($"Public IP: { XMLUtils.ComputerPublicIP }");

                            File.WriteAllText(Path.Combine(BYR_File_Dir, $"Computer_Info_{ DateTime.UtcNow.ToString("yyyy-dd-M__HH-mm-ss") }.txt"), compInfo.ToString());
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion

                            #region Save Project
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            File.WriteAllBytes(Path.Combine(BYR_File_Dir, "EXGuard_Project.exproj"), EXGuard_Project_MemoryStream.ToArray());
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #endregion
                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        }
                        else
                            File.WriteAllText(Path.Combine(CrashLog_PHP_Directory, "conerr.log"), "Login Failed.");

                        EXGuard_Project_MemoryStream.Close();
                        EXGuard_Project_MemoryStream.Dispose();

                        EXGuard_Project_Stream.Close();
                        EXGuard_Project_Stream.Dispose();

                        archive.Dispose();
                    }

                    archiveStream.Close();
                    archiveStream.Dispose();
                }
            });

            ThreadA.Start();
            ThreadA.Join();

            Thread.Sleep(2000);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var ThreadB = new Thread(() =>
            {
                if (KeyAuthApp.response.success)
                {
                    #region Archive Protected File
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    using (var archive = new ZipFile())
                    {
                        archive.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;

                        foreach (var entry in archive.Entries)
                            archive.Entries.Remove(entry);

                        foreach (var protectedFile in Directory.GetFiles(Protected_File_Dir))
                            archive.AddEntry(Path.GetFileName(protectedFile), File.ReadAllBytes(protectedFile));

                        if (File.Exists(args[1]))
                            File.Delete(args[1]);

                        var encryptedArchive = new MemoryStream();
                        archive.Save(encryptedArchive);

                        encryptedArchive = new MemoryStream(ArchiveEncryptionAlgorithm.Encrypt(encryptedArchive.ToArray()));
                        File.WriteAllBytes(args[1], encryptedArchive.ToArray());

                        Thread.Sleep(1000);

                        archive.Dispose();
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion
                }
            });

            ThreadB.Start();
            ThreadB.Join();

            Thread.Sleep(2000);
        }
    }
}
