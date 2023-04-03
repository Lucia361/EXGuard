using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace EXGuard.Console.Services
{
    public static class ArchiveEncryptionAlgorithm
    {
        static uint[] Keys()
        {
            uint[] keys = new uint[2];

            keys[0] = 1276489254;
            keys[1] = 2142154667;

            return keys;
        }

        public static byte[] Encrypt(byte[] data)
        {
            uint[] dst = new uint[0x10];
            uint[] src = new uint[0x10];

            ulong state = Keys()[0]; // Seed
            for (int i = 0; i < 0x10; i++)
            {
                state = (state * state) % 0x143fc089;

                src[i] = (uint)state;
                dst[i] = (uint)((state * state) % 0x444d56fb);
            }

            // Mutation.Crypt
            uint[] key = new uint[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        key[i] = dst[i] ^ src[i];
                        break;
                    case 1:
                        key[i] = dst[i] * src[i];
                        break;
                    case 2:
                        key[i] = dst[i] + src[i];
                        break;
                }
            }
            ///////////////////////////////////////////////

            for (int i = 0; i < 0x10; i++)
            {
                state ^= state >> 13;
                state ^= state << 25;
                state ^= state >> 27;

                src[i] = 0;
                dst[i] = 0;

                switch (i % 3)
                {
                    case 0:
                        key[i] = key[i] ^ (uint)state;
                        break;
                    case 1:
                        key[i] = key[i] * (uint)state;
                        break;
                    case 2:
                        key[i] = key[i] + (uint)state;
                        break;
                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)state;

                if ((i & 0xff) == 0)
                    state = (state * state) % 0x8a5cb7;
            }

            byte[] savedData = data;
            int newSize = (data.Length + 3) & ~3;

            if (savedData == null)
                data = new byte[newSize];
            else if (savedData.Length != newSize)
            {
                byte[] dummyArray = new byte[newSize];

                for (int i = 0; i < ((savedData.Length > newSize) ? newSize : savedData.Length); i++)
                    dummyArray[i] = savedData[i];

                data = dummyArray;
            }

            byte[] encryptedData = new byte[data.Length];
            int keyIndex = 0;
            for (int i = 0; i < data.Length; i += 4)
            {
                uint datum = (uint)(data[i + 0] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24));
                uint encrypted = datum ^ key[keyIndex & 0xf];

                key[keyIndex & 0xf] = (key[keyIndex & 0xf] ^ datum) + 0x3ddb2819;

                encryptedData[i + 0] = (byte)(encrypted >> 0);
                encryptedData[i + 1] = (byte)(encrypted >> 8);
                encryptedData[i + 2] = (byte)(encrypted >> 16);
                encryptedData[i + 3] = (byte)(encrypted >> 24);

                keyIndex++;
            }

            return encryptedData;
        }

        public static byte[] Decrypt(byte[] data)
        {
            int decodeLen = data.Length / 4;
            uint[] decode = new uint[decodeLen];
            Buffer.BlockCopy(data, 0, decode, 0, data.Length);

            uint[] dst = new uint[0x10];
            uint[] src = new uint[0x10];

            ulong state = Keys()[1]; // Seed
            for (int i = 0; i < 0x10; i++)
            {
                state = (state * state) % 0x143fc089;

                src[i] = (uint)state;
                dst[i] = (uint)((state * state) % 0x444d56fb);
            }

            // Mutation.Crypt
            dst[0] = dst[0] ^ src[0];
            dst[1] = dst[1] * src[1];
            dst[2] = dst[2] + src[2];
            dst[3] = dst[3] ^ src[3];
            dst[4] = dst[4] * src[4];
            dst[5] = dst[5] + src[5];
            dst[6] = dst[6] ^ src[6];
            dst[7] = dst[7] * src[7];
            dst[8] = dst[8] + src[8];
            dst[9] = dst[9] ^ src[9];
            dst[10] = dst[10] * src[10];
            dst[11] = dst[11] + src[11];
            dst[12] = dst[12] ^ src[12];
            dst[13] = dst[13] * src[13];
            dst[14] = dst[14] + src[14];
            dst[15] = dst[15] ^ src[15];
            ////////////////////////////////////

            for (int i = 0; i < 0x10; i++)
            {
                state ^= state >> 13;
                state ^= state << 25;
                state ^= state >> 27;

                src[i] = 0;

                switch (i % 3)
                {
                    case 0:
                        dst[i] = dst[i] ^ (uint)state;
                        break;
                    case 1:
                        dst[i] = dst[i] * (uint)state;
                        break;
                    case 2:
                        dst[i] = dst[i] + (uint)state;
                        break;
                }
            }

            int decryptedDataLen = decodeLen << 2;
            byte[] decryptedData = new byte[decryptedDataLen];

            uint h = 0;
            for (int i = 0; i < decodeLen; i++)
            {
                uint decrypted = decode[i] ^ dst[i & 0xf];

                dst[i & 0xf] = (dst[i & 0xf] ^ decrypted) + 0x3ddb2819;

                decryptedData[h + 0] = (byte)(decrypted >> 0);
                decryptedData[h + 1] = (byte)(decrypted >> 8);
                decryptedData[h + 2] = (byte)(decrypted >> 16);
                decryptedData[h + 3] = (byte)(decrypted >> 24);

                h += 4;
            }

            for (int i = 0; i < 0x10; i++)
                dst[i] = 0;

            for (int i = 0; i < decryptedDataLen; i++)
            {
                decryptedData[i] ^= (byte)state;

                if ((i & 0xff) == 0)
                    state = (state * state) % 0x8a5cb7;
            }

            return decryptedData;
        }
    }
}
