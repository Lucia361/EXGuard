using System;
using System.IO;
using System.IO.Compression;

using dnlib.IO;
using dnlib.DotNet;

namespace EXGuard.Console.Services
{
    public class CosturaFodyDecompressor
    {
        public static void ExtractDLLs(ModuleDefMD mod, string extractDir)
        {
            if (mod.HasResources)
            {
                foreach (Resource resource in mod.Resources)
                {
                    if (resource.Name.StartsWith("costura.") && resource.Name.EndsWith(".dll.compressed"))
                    {
                        EmbeddedResource DLL = mod.Resources.FindEmbeddedResource(resource.Name);
                        DataReader reader = DLL.CreateReader();
                        string name = fixName(resource.Name);
                        using (Stream bufferStream = reader.AsStream())
                        {
                            string path = Path.Combine(extractDir, name);
                            File.WriteAllBytes(path, DecompressResource(bufferStream));
                        }
                    }
                }
            }
        }

        private static string fixName(string name)
        {
            name = name.Replace(".dll.compressed", ".dll");
            if (name == "costura.costura.dll")
                name = name.Replace("costura.costura", "costura");
            else
                name = name.Replace("costura.", string.Empty);
            return name;
        }

        private static byte[] DecompressResource(Stream input)
        {
            MemoryStream output = new MemoryStream();
            new DeflateStream(input, CompressionMode.Decompress).CopyTo(output);

            return output.ToArray();
        }
    }
}
