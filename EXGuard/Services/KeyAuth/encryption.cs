using System;
using System.Text;
using System.Security.Cryptography;

namespace KeyAuth
{
    public static class encryption
    {
        public static string HashHMAC(string enckey, string resp)
        {
            byte[] key = Encoding.ASCII.GetBytes(enckey);
            byte[] message = Encoding.ASCII.GetBytes(resp);
            var hash = new HMACSHA256(key);
            return byte_arr_to_str(hash.ComputeHash(message));
        }

        public static string byte_arr_to_str(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] str_to_byte_arr(string hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch
            {
                api.error("The session has ended, open program again.");
                Environment.Exit(0);
                return null;
            }
        }

        public static string iv_key() =>
            Guid.NewGuid().ToString().Substring(0, 16);
    }
}
