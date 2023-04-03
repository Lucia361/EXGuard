using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Collections.Generic;

namespace EXGuard.Console
{
    public static class XMLUtils
    {
        public static IList<string> Methods_FullNames { get; internal set; } = new List<string>();
        public static IList<int> Methods_Tokens { get; internal set; } = new List<int>();

        public static string UserName { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;

        public static string ComputerOS { get; set; } = string.Empty;
        public static string ComputerUserName { get; set; } = string.Empty;
        public static string ComputerRamSize { get; set; } = string.Empty;
        public static string ComputerCPUName { get; set; } = string.Empty;
        public static string ComputerPublicIP { get; set; } = string.Empty;

        public static bool AntiDebug { get; set; } = false;
        public static bool AntiDump { get; set; } = false;
        public static bool AntiVM { get; set; } = false;
        public static bool AntiILDasm { get; set; } = false;
        public static bool AntiDe4dot { get; set; } = false;
        public static bool AntiDnspy { get; set; } = false;
        public static bool AntiWebDebuggers { get; set; } = false;

        public static bool VirtualizeAllStrings { get; set; } = false;
        public static bool VirtualizeAllNumbers { get; set; } = false;
        public static bool VirtualizeAllReferenceProxies { get; set; } = false;
        public static bool CodeMutation { get; set; } = false;
        public static bool CodeFlow { get; set; } = false;

        public static bool ResourceCompressAndEncrypt { get; set; } = false;

        public static string SNKFile_Name { get; set; } = string.Empty;
        public static byte[] SNKFile_Bytes { get; set; } = new byte[0];
        public static string SNK_Password { get; set; } = null;

        public static string InputFileName { get; set; } = string.Empty;
        public static string RuntimeName { get; set; } = string.Empty;

        public static void Read(MemoryStream xmlFile)
        {
            var thread = new Thread(() =>
            {
                if (xmlFile != null)
                {
                    var EXGuard_Config = XDocument.Load(new StreamReader(xmlFile)).Element("EXGuard");

                    #region Read UserName and Password
                    ////////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("UserName").IsEmpty == false)
                        UserName = EXGuard_Config.Element("UserName").Value;

                    if (EXGuard_Config.Element("Password").IsEmpty == false)
                        Password = EXGuard_Config.Element("Password").Value;
                    ////////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Read Computer Info
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("ComputerOS").IsEmpty == false)
                        ComputerOS = EXGuard_Config.Element("ComputerOS").Value;

                    if (EXGuard_Config.Element("ComputerUserName").IsEmpty == false)
                        ComputerUserName = EXGuard_Config.Element("ComputerUserName").Value;

                    if (EXGuard_Config.Element("ComputerRamSize").IsEmpty == false)
                        ComputerRamSize = EXGuard_Config.Element("ComputerRamSize").Value;

                    if (EXGuard_Config.Element("ComputerCPUName").IsEmpty == false)
                        ComputerCPUName = EXGuard_Config.Element("ComputerCPUName").Value;

                    if (EXGuard_Config.Element("ComputerPublicIP").IsEmpty == false)
                        ComputerPublicIP = EXGuard_Config.Element("ComputerPublicIP").Value;
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Read Protection Settings
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("AntiDebug").IsEmpty == false)
                        AntiDebug = Convert.ToBoolean(EXGuard_Config.Element("AntiDebug").Value);

                    if (EXGuard_Config.Element("AntiDump").IsEmpty == false)
                        AntiDump = Convert.ToBoolean(EXGuard_Config.Element("AntiDump").Value);

                    if (EXGuard_Config.Element("AntiVM").IsEmpty == false)
                        AntiVM = Convert.ToBoolean(EXGuard_Config.Element("AntiVM").Value);

                    if (EXGuard_Config.Element("AntiILDasm").IsEmpty == false)
                        AntiILDasm = Convert.ToBoolean(EXGuard_Config.Element("AntiILDasm").Value);

                    if (EXGuard_Config.Element("AntiDe4dot").IsEmpty == false)
                        AntiDe4dot = Convert.ToBoolean(EXGuard_Config.Element("AntiDe4dot").Value);

                    if (EXGuard_Config.Element("AntiDnspy").IsEmpty == false)
                        AntiDnspy = Convert.ToBoolean(EXGuard_Config.Element("AntiDnspy").Value);

                    if (EXGuard_Config.Element("AntiWebDebuggers").IsEmpty == false)
                        AntiWebDebuggers = Convert.ToBoolean(EXGuard_Config.Element("AntiWebDebuggers").Value);

                    if (EXGuard_Config.Element("VirtualizeAllStrings").IsEmpty == false)
                        VirtualizeAllStrings = Convert.ToBoolean(EXGuard_Config.Element("VirtualizeAllStrings").Value);

                    if (EXGuard_Config.Element("VirtualizeAllNumbers").IsEmpty == false)
                        VirtualizeAllNumbers = Convert.ToBoolean(EXGuard_Config.Element("VirtualizeAllNumbers").Value);

                    if (EXGuard_Config.Element("VirtualizeAllReferenceProxies").IsEmpty == false)
                        VirtualizeAllReferenceProxies = Convert.ToBoolean(EXGuard_Config.Element("VirtualizeAllReferenceProxies").Value);

                    if (EXGuard_Config.Element("CodeMutation").IsEmpty == false)
                        CodeMutation = Convert.ToBoolean(EXGuard_Config.Element("CodeMutation").Value);

                    if (EXGuard_Config.Element("CodeFlow").IsEmpty == false)
                        CodeFlow = Convert.ToBoolean(EXGuard_Config.Element("CodeFlow").Value);

                    if (EXGuard_Config.Element("ResourceCompressAndEncrypt").IsEmpty == false)
                        ResourceCompressAndEncrypt = Convert.ToBoolean(EXGuard_Config.Element("ResourceCompressAndEncrypt").Value);
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Load SNK File Bytes And Password
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("SNKFile_Name").IsEmpty == false)
                        SNK_Password = EXGuard_Config.Element("SNKFile_Name").Value;

                    if (EXGuard_Config.Element("SNKFile_Bytes").IsEmpty == false)
                        SNKFile_Bytes = Convert.FromBase64String(EXGuard_Config.Element("SNKFile_Bytes").Value);

                    if (EXGuard_Config.Element("SNKPassword").IsEmpty == false)
                        SNK_Password = EXGuard_Config.Element("SNKPassword").Value;
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Read Input File Name
                    //////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("InputFileName").IsEmpty == false)
                        InputFileName = EXGuard_Config.Element("InputFileName").Value;
                    //////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Read Runtime Dll Name
                    //////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("RuntimeName").IsEmpty == false)
                        RuntimeName = EXGuard_Config.Element("RuntimeName").Value;
                    //////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    #region Read Virtualize Methods Tokens and FullNames
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (EXGuard_Config.Element("Virtualize").IsEmpty == false)
                    {
                        foreach (XElement fullName in EXGuard_Config.Element("Virtualize").Element("Methods_FullNames").Elements("FullName"))
                            Methods_FullNames.Add(fullName.Value);

                        foreach (XElement mdToken in EXGuard_Config.Element("Virtualize").Element("Methods").Elements("MDToken"))
                            Methods_Tokens.Add(int.Parse(mdToken.Value));
                    }

                    #region Delete if there are two of the same
                    /////////////////////////////
                    Methods_Tokens.Distinct();
                    /////////////////////////////
                    #endregion
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    xmlFile.Flush();
                    xmlFile.Close();
                }
            });

            thread.Start();
            thread.Join();
        }
    }
}
