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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using dnlib.DotNet;

using Ionic.Zip;

using EXGuard.Internal;

namespace EXGuard.Console
{
    public static unsafe class Utils
    {
        public static MemoryStream ToMemoryStream(this Stream stream, bool disposeScrStream = false)
        {
            var retStream = new MemoryStream();
            stream.CopyTo(retStream, (int)stream.Length);

            retStream.Position = 0;

            if (disposeScrStream)
                stream.Dispose();

            return retStream;
        }
        public static void CopyTo(this Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        public static ZipEntry GetEntry(this ZipFile zip, string entryName)
        {
            foreach (var entry in zip)
            {
                if (entry.FileName == entryName)
                    return entry;
            }

            return null;
        }

        public static string File_Create_Murmur2(byte[] inputBytes)
        {
            ulong length = (ulong)inputBytes.Length;

            ulong m = 0xc6a4a7935bd1e995L;
            ulong r = 47;
            ulong h = 0xffffffffL ^ (length * m);
            ulong length8 = length / 8;

            for (UInt32 i = 0; i < length8; i++)
            {
                ulong i8 = i * 8;
                ulong k = ((ulong)inputBytes[i8 + 0] & 0xff) + (((ulong)inputBytes[i8 + 1] & 0xff) << 8)
                    + (((ulong)inputBytes[i8 + 2] & 0xff) << 16) + (((ulong)inputBytes[i8 + 3] & 0xff) << 24)
                    + (((ulong)inputBytes[i8 + 4] & 0xff) << 32) + (((ulong)inputBytes[i8 + 5] & 0xff) << 40)
                    + (((ulong)inputBytes[i8 + 6] & 0xff) << 48) + (((ulong)inputBytes[i8 + 7] & 0xff) << 56);

                k *= m;
                k ^= k >> (Int16)r;
                k *= m;

                h ^= k;
                h *= m;
            }

            switch (length % 8)
            {
                case 7:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 6] & 0xff) << 48;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 5] & 0xff) << 40;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 4] & 0xff) << 32;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 3] & 0xff) << 24;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 2] & 0xff) << 16;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 6:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 5] & 0xff) << 40;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 4] & 0xff) << 32;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 3] & 0xff) << 24;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 2] & 0xff) << 16;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 5:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 4] & 0xff) << 32;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 3] & 0xff) << 24;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 2] & 0xff) << 16;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 4:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 3] & 0xff) << 24;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 2] & 0xff) << 16;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 3:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 2] & 0xff) << 16;
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 2:
                    h ^= (ulong)(inputBytes[(length & ~(ulong)7) + 1] & 0xff) << 8;
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
                case 1:
                    h ^= (ulong)(inputBytes[length & ~(ulong)7] & 0xff);
                    h *= m;
                    break;
            };

            h ^= h >> (short)r;
            h *= m;
            h ^= h >> (short)r;

            byte[] hashBytes = new byte[8];

            fixed (byte* b = hashBytes)
                *((ulong*)b) = h;

            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }

        public static string File_Create_MD5(string inputFile)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = File.ReadAllBytes(inputFile);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("x2"));

                return sb.ToString();
            }
        }

        public static string BytesSizeRead(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

            if (byteCount == 0)
                return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }
    }
}
