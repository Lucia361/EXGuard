using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EXGuard
{
    public static class Utils
    {
		[DllImport("kernel32.dll", EntryPoint = "GetPhysicallyInstalledSystemMemory")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

		[DllImport("wininet.dll", EntryPoint = "InternetGetConnectedState")]
		public extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

		public static bool IsDotNetAssembly(string assemblyPath)
		{
			bool result;
			try
			{
				System.Reflection.AssemblyName.GetAssemblyName(assemblyPath);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		public static MemoryStream ToMemoryStream(this Stream stream, bool disposeScrStream = false)
		{
			var retStream = new MemoryStream();
			stream.CopyTo(retStream);

			retStream.Position = 0;

			if (disposeScrStream)
				stream.Dispose();

			return retStream;
		}
	}
}
