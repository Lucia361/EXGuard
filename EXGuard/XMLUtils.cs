using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EXGuard
{
	[System.Reflection.Obfuscation(Feature = "Apply to member * when method or constructor: virtualization", Exclude = false)]
	internal static class XMLUtils
	{
		public static IList<string> Virt_Methods_FullNames = new List<string>();
		public static IList<int> Virt_Methods_Tokens = new List<int>();

		public static string UserName = string.Empty;
		public static string Password = string.Empty;

		public static bool AntiDebug = false;
		public static bool AntiDump = false;
		public static bool AntiVM = false;
		public static bool AntiILDasm = false;
		public static bool AntiDe4dot = false;
		public static bool AntiDnspy = false;
		public static bool AntiWebDebuggers = false;

		public static bool VirtualizeAllStrings = false;
		public static bool VirtualizeAllNumbers = false;
		public static bool VirtualizeAllReferenceProxies = false;
		public static bool CodeMutation = false;
		public static bool CodeFlow = false;

		public static bool ResourceCompressAndEncrypt = false;

		public static string SNKFile_Name = string.Empty;
		public static byte[] SNKFile_Bytes = new byte[0];
		public static string SNK_Password = string.Empty;

		public static string InputFileName = string.Empty;
		public static string RuntimeName = "EXGuard.dll";

        public static void ResetXMLProject()
		{
			Virt_Methods_FullNames = new List<string>();
			Virt_Methods_Tokens = new List<int>();

			//UserName = string.Empty;
			//Password = string.Empty;

			AntiDebug = false;
			AntiDump = false;
			AntiVM = false;
			AntiILDasm = false;
			AntiDe4dot = false;
			AntiDnspy = false;
			AntiWebDebuggers = false;

			VirtualizeAllStrings = false;
			VirtualizeAllNumbers = false;
			VirtualizeAllReferenceProxies = false;
			CodeMutation = false;
			CodeFlow = false;

			ResourceCompressAndEncrypt = false;

			SNKFile_Name = string.Empty;
			SNKFile_Bytes = new byte[0];
			SNK_Password = string.Empty;

			InputFileName = string.Empty;
			RuntimeName = "EXGuard.dll";
        }

        public static void ReadProject(MemoryStream xmlFile)
        {

        }

        public static bool SaveProject(Stream EXGuard_Config)
		{
			using (XmlWriter writer = XmlWriter.Create(EXGuard_Config, new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "\t",
				CloseOutput = false,
				OmitXmlDeclaration = true
			}))
			{
				writer.WriteStartElement("EXGuard");

				writer.WriteElementString("UserName", UserName);
				writer.WriteElementString("Password", Password);

				// Computer Info
				writer.WriteElementString("ComputerOS", RuntimeInformation.OSDescription);
				writer.WriteElementString("ComputerUserName", WindowsIdentity.GetCurrent().Name);

				long memKb;
				Utils.GetPhysicallyInstalledSystemMemory(out memKb);
				writer.WriteElementString("ComputerRamSize", $"{ (memKb / 1024 / 1024) } GB");

				ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
				foreach (ManagementObject mo in mos.Get())
					writer.WriteElementString("ComputerCPUName", (string)mo["Name"]);

				writer.WriteElementString("ComputerPublicIP", new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim());
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// Antis
				writer.WriteElementString("AntiDebug", AntiDebug.ToString());
				writer.WriteElementString("AntiDump", AntiDump.ToString());
				writer.WriteElementString("AntiVM", AntiVM.ToString());
				writer.WriteElementString("AntiILDasm", AntiILDasm.ToString());
				writer.WriteElementString("AntiDe4dot", AntiDe4dot.ToString());
				writer.WriteElementString("AntiDnspy", AntiDnspy.ToString());
				writer.WriteElementString("AntiWebDebuggers", AntiWebDebuggers.ToString());
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// Misc
				writer.WriteElementString("VirtualizeAllStrings", VirtualizeAllStrings.ToString());
				writer.WriteElementString("VirtualizeAllNumbers", VirtualizeAllNumbers.ToString());
				writer.WriteElementString("VirtualizeAllReferenceProxies", VirtualizeAllReferenceProxies.ToString());
				writer.WriteElementString("CodeMutation", CodeMutation.ToString());
				writer.WriteElementString("CodeFlow", CodeFlow.ToString());
				writer.WriteElementString("ResourceCompressAndEncrypt", ResourceCompressAndEncrypt.ToString());
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				writer.WriteElementString("InputFileName", InputFileName);
				writer.WriteElementString("RuntimeName", RuntimeName);
			
				#region Write SNK File Bytes And Password
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////
				writer.WriteElementString("SNKFile_Name", SNKFile_Name);

				if (SNKFile_Bytes.Length != 0)
					writer.WriteElementString("SNKFile_Bytes", Convert.ToBase64String(SNKFile_Bytes));
				else
					writer.WriteElementString("SNKFile_Bytes", string.Empty);

				writer.WriteElementString("SNKPassword", SNK_Password);
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////
				#endregion

				#region Write Virtualize Methods Tokens and FullNames
				////////////////////////////////////////////////////////////////////////////////////
				writer.WriteStartElement("Virtualize");

				////////////////////////////////////////////////////////////////////////////////////
				
				writer.WriteStartElement("Methods");

				if (Virt_Methods_Tokens != null)
					foreach (var md in Virt_Methods_Tokens.Distinct())
						writer.WriteElementString("MDToken", md.ToString());

				writer.WriteEndElement();

				////////////////////////////////////////////////////////////////////////////////////

				writer.WriteStartElement("Methods_FullNames");

				if (Virt_Methods_FullNames != null)
					foreach (var md in Virt_Methods_FullNames.Distinct())
						writer.WriteElementString("FullName", md.ToString());

				writer.WriteEndElement();
				////////////////////////////////////////////////////////////////////////////////////
				#endregion

				writer.Flush();
				writer.Close();
			}

			return true;
		}
	}
}
