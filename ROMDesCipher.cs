using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ROMEncryption
{
    public class ROMDesCipher
    {
        public static Byte[] ROMKey = new Byte[] { 2, 5, 9, 3, 6, 1, 0, 1 };
        public static String ROMSig = "czjzgqde";
        // without unity header
        public static void DecryptFile(String file)
        {
            var fileBytes = File.ReadAllBytes(file);
            var header = fileBytes.Take(12);
            var signature = header.Take(8);
            if (Encoding.ASCII.GetString(signature.ToArray()) != ROMSig)
                throw new Exception("not a valid ROM payload");
            var size = BitConverter.ToInt32(header.Skip(8).ToArray(), 0);
            var encryptedBytes = fileBytes.Skip(12).ToArray();
            var cipher = new ROMDesCipher(ROMKey, 8);
            var decryptedBytes = cipher.Decrypt(encryptedBytes);
            File.WriteAllBytes(file.replace(".bytes","") + ".lua", decryptedBytes.Take(size).ToArray());
        }
        // with unity header
        public static void EncryptFile(String file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var unityBlob = new List<Byte>();
            unityBlob.AddRange(BitConverter.GetBytes(fileName.Length));
            unityBlob.AddRange(Encoding.ASCII.GetBytes(fileName));
            var pad = 8 - ((unityBlob.Count - 1) % 8) - 1;
            unityBlob.AddRange(Enumerable.Range(0, pad).Select(i => (Byte)0).ToList());

            var bytes = File.ReadAllBytes(file).ToList();
            var size = bytes.Count;
            pad = 8 - ((bytes.Count - 1) % 8) - 1;
            bytes.AddRange(Enumerable.Range(0, pad).Select(i => (Byte)0).ToList());
            var cipher = new ROMDesCipher(ROMKey, 8);
            var encryptedBytes = cipher.Encrypt(bytes.ToArray());
            var payload = new List<Byte>();
            payload.AddRange(Encoding.ASCII.GetBytes(ROMSig));
            payload.AddRange(BitConverter.GetBytes(size));
            payload.AddRange(encryptedBytes);

            unityBlob.AddRange(BitConverter.GetBytes(payload.Count));
            unityBlob.AddRange(payload);

            File.WriteAllBytes(file.replace(".lua","") + ".bytes", unityBlob.ToArray());
        }
        private UInt32[] EncryptionKey;
        private UInt32[] DecryptionKey;
        public Byte[] Key;
        public UInt16 BlockSize;
        private static readonly Byte[] PermutatedChoice1 =
        {
            0x38, 0x30, 0x28, 0x20, 0x18, 0x10, 0x08, 0x00, 0x39, 0x31, 0x29, 0x21, 0x19, 0x11, 0x09, 0x01,
            0x3A, 0x32, 0x2A, 0x22, 0x1A, 0x12, 0x0A, 0x02, 0x3B, 0x33, 0x2B, 0x23, 0x3E, 0x36, 0x2E, 0x26,
            0x1E, 0x16, 0x0E, 0x06, 0x3D, 0x35, 0x2D, 0x25, 0x1D, 0x15, 0x0D, 0x05, 0x3C, 0x34, 0x2C, 0x24,
            0x1C, 0x14, 0x0C, 0x04, 0x1B, 0x13, 0x0B, 0x03
        };

        private static readonly Byte[] NumLeftRotations =
        {
            0x01, 0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e,
            0x0f, 0x11, 0x13, 0x15, 0x17, 0x19, 0x1b, 0x1c
        };

        private static readonly Byte[] PermutatedChoice2 =
        {
            0x0D, 0x10, 0x0A, 0x17, 0x00, 0x04, 0x02, 0x1B, 0x0E, 0x05, 0x14, 0x09, 0x16, 0x12, 0x0B, 0x03,
            0x19, 0x07, 0x0F, 0x06, 0x1A, 0x13, 0x0C, 0x01, 0x28, 0x33, 0x1E, 0x24, 0x2E, 0x36, 0x1D, 0x27,
            0x32, 0x2C, 0x20, 0x2F, 0x2B, 0x30, 0x26, 0x37, 0x21, 0x34, 0x2D, 0x29, 0x31, 0x23, 0x1C, 0x1F
        };
        private static readonly UInt32[,] SPBox =
        { {
            0x01010400, 0x00000000, 0x00010000, 0x01010404, 0x01010004, 0x00010404, 0x00000004, 0x00010000,
            0x00000400, 0x01010400, 0x01010404, 0x00000400, 0x01000404, 0x01010004, 0x01000000, 0x00000004,
            0x00000404, 0x01000400, 0x01000400, 0x00010400, 0x00010400, 0x01010000, 0x01010000, 0x01000404,
            0x00010004, 0x01000004, 0x01000004, 0x00010004, 0x00000000, 0x00000404, 0x00010404, 0x01000000,
            0x00010000, 0x01010404, 0x00000004, 0x01010000, 0x01010400, 0x01000000, 0x01000000, 0x00000400,
            0x01010004, 0x00010000, 0x00010400, 0x01000004, 0x00000400, 0x00000004, 0x01000404, 0x00010404,
            0x01010404, 0x00010004, 0x01010000, 0x01000404, 0x01000004, 0x00000404, 0x00010404, 0x01010400,
            0x00000404, 0x01000400, 0x01000400, 0x00000000, 0x00010004, 0x00010400, 0x00000000, 0x01010004
        },{
            0x80108020, 0x80008000, 0x00008000, 0x00108020, 0x00100000, 0x00000020, 0x80100020, 0x80008020,
            0x80000020, 0x80108020, 0x80108000, 0x80000000, 0x80008000, 0x00100000, 0x00000020, 0x80100020,
            0x00108000, 0x00100020, 0x80008020, 0x00000000, 0x80000000, 0x00008000, 0x00108020, 0x80100000,
            0x00100020, 0x80000020, 0x00000000, 0x00108000, 0x00008020, 0x80108000, 0x80100000, 0x00008020,
            0x00000000, 0x00108020, 0x80100020, 0x00100000, 0x80008020, 0x80100000, 0x80108000, 0x00008000,
            0x80100000, 0x80008000, 0x00000020, 0x80108020, 0x00108020, 0x00000020, 0x00008000, 0x80000000,
            0x00008020, 0x80108000, 0x00100000, 0x80000020, 0x00100020, 0x80008020, 0x80000020, 0x00100020,
            0x00108000, 0x00000000, 0x80008000, 0x00008020, 0x80000000, 0x80100020, 0x80108020, 0x00108000
        },{
            0x00000208, 0x08020200, 0x00000000, 0x08020008, 0x08000200, 0x00000000, 0x00020208, 0x08000200,
            0x00020008, 0x08000008, 0x08000008, 0x00020000, 0x08020208, 0x00020008, 0x08020000, 0x00000208,
            0x08000000, 0x00000008, 0x08020200, 0x00000200, 0x00020200, 0x08020000, 0x08020008, 0x00020208,
            0x08000208, 0x00020200, 0x00020000, 0x08000208, 0x00000008, 0x08020208, 0x00000200, 0x08000000,
            0x08020200, 0x08000000, 0x00020008, 0x00000208, 0x00020000, 0x08020200, 0x08000200, 0x00000000,
            0x00000200, 0x00020008, 0x08020208, 0x08000200, 0x08000008, 0x00000200, 0x00000000, 0x08020008,
            0x08000208, 0x00020000, 0x08000000, 0x08020208, 0x00000008, 0x00020208, 0x00020200, 0x08000008,
            0x08020000, 0x08000208, 0x00000208, 0x08020000, 0x00020208, 0x00000008, 0x08020008, 0x00020200
        },{
            0x00802001, 0x00002081, 0x00002081, 0x00000080, 0x00802080, 0x00800081, 0x00800001, 0x00002001,
            0x00000000, 0x00802000, 0x00802000, 0x00802081, 0x00000081, 0x00000000, 0x00800080, 0x00800001,
            0x00000001, 0x00002000, 0x00800000, 0x00802001, 0x00000080, 0x00800000, 0x00002001, 0x00002080,
            0x00800081, 0x00000001, 0x00002080, 0x00800080, 0x00002000, 0x00802080, 0x00802081, 0x00000081,
            0x00800080, 0x00800001, 0x00802000, 0x00802081, 0x00000081, 0x00000000, 0x00000000, 0x00802000,
            0x00002080, 0x00800080, 0x00800081, 0x00000001, 0x00802001, 0x00002081, 0x00002081, 0x00000080,
            0x00802081, 0x00000081, 0x00000001, 0x00002000, 0x00800001, 0x00002001, 0x00802080, 0x00800081,
            0x00002001, 0x00002080, 0x00800000, 0x00802001, 0x00000080, 0x00800000, 0x00002000, 0x00802080
        },{
            0x00000100, 0x02080100, 0x02080000, 0x42000100, 0x00080000, 0x00000100, 0x40000000, 0x02080000,
            0x40080100, 0x00080000, 0x02000100, 0x40080100, 0x42000100, 0x42080000, 0x00080100, 0x40000000,
            0x02000000, 0x40080000, 0x40080000, 0x00000000, 0x40000100, 0x42080100, 0x42080100, 0x02000100,
            0x42080000, 0x40000100, 0x00000000, 0x42000000, 0x02080100, 0x02000000, 0x42000000, 0x00080100,
            0x00080000, 0x42000100, 0x00000100, 0x02000000, 0x40000000, 0x02080000, 0x42000100, 0x40080100,
            0x02000100, 0x40000000, 0x42080000, 0x02080100, 0x40080100, 0x00000100, 0x02000000, 0x42080000,
            0x42080100, 0x00080100, 0x42000000, 0x42080100, 0x02080000, 0x00000000, 0x40080000, 0x42000000,
            0x00080100, 0x02000100, 0x40000100, 0x00080000, 0x00000000, 0x40080000, 0x02080100, 0x40000100
        },{
            0x20000010, 0x20400000, 0x00004000, 0x20404010, 0x20400000, 0x00000010, 0x20404010, 0x00400000,
            0x20004000, 0x00404010, 0x00400000, 0x20000010, 0x00400010, 0x20004000, 0x20000000, 0x00004010,
            0x00000000, 0x00400010, 0x20004010, 0x00004000, 0x00404000, 0x20004010, 0x00000010, 0x20400010,
            0x20400010, 0x00000000, 0x00404010, 0x20404000, 0x00004010, 0x00404000, 0x20404000, 0x20000000,
            0x20004000, 0x00000010, 0x20400010, 0x00404000, 0x20404010, 0x00400000, 0x00004010, 0x20000010,
            0x00400000, 0x20004000, 0x20000000, 0x00004010, 0x20000010, 0x20404010, 0x00404000, 0x20400000,
            0x00404010, 0x20404000, 0x00000000, 0x20400010, 0x00000010, 0x00004000, 0x20400000, 0x00404010,
            0x00004000, 0x00400010, 0x20004010, 0x00000000, 0x20404000, 0x20000000, 0x00400010, 0x20004010
        },{
            0x00200000, 0x04200002, 0x04000802, 0x00000000, 0x00000800, 0x04000802, 0x00200802, 0x04200800,
            0x04200802, 0x00200000, 0x00000000, 0x04000002, 0x00000002, 0x04000000, 0x04200002, 0x00000802,
            0x04000800, 0x00200802, 0x00200002, 0x04000800, 0x04000002, 0x04200000, 0x04200800, 0x00200002,
            0x04200000, 0x00000800, 0x00000802, 0x04200802, 0x00200800, 0x00000002, 0x04000000, 0x00200800,
            0x04000000, 0x00200800, 0x00200000, 0x04000802, 0x04000802, 0x04200002, 0x04200002, 0x00000002,
            0x00200002, 0x04000000, 0x04000800, 0x00200000, 0x04200800, 0x00000802, 0x00200802, 0x04200800,
            0x00000802, 0x04000002, 0x04200802, 0x04200000, 0x00200800, 0x00000000, 0x00000002, 0x04200802,
            0x00000000, 0x00200802, 0x04200000, 0x00000800, 0x04000002, 0x04000800, 0x00000800, 0x00200002
        },{
            0x10001040, 0x00001000, 0x00040000, 0x10041040, 0x10000000, 0x10001040, 0x00000040, 0x10000000,
            0x00040040, 0x10040000, 0x10041040, 0x00041000, 0x10041000, 0x00041040, 0x00001000, 0x00000040,
            0x10040000, 0x10000040, 0x10001000, 0x00001040, 0x00041000, 0x00040040, 0x10040040, 0x10041000,
            0x00001040, 0x00000000, 0x00000000, 0x10040040, 0x10000040, 0x10001000, 0x00041040, 0x00040000,
            0x00041040, 0x00040000, 0x10041000, 0x00001000, 0x00000040, 0x10040040, 0x00001000, 0x00041040,
            0x10001000, 0x00000040, 0x10000040, 0x10040000, 0x10040040, 0x10000000, 0x00040000, 0x10001040,
            0x00000000, 0x10041040, 0x00040040, 0x10000040, 0x10040000, 0x10001000, 0x10001040, 0x00000000,
            0x10041040, 0x00041000, 0x00041000, 0x00001040, 0x00001040, 0x00040040, 0x10000000, 0x10041000
        } };
        public ROMDesCipher(Byte[] key, UInt16 blockSize)
        {
            Key = key;
            BlockSize = blockSize;
            EncryptionKey = GenerateKey(true, Key);
            DecryptionKey = GenerateKey(false, Key);
        }
        public Byte[] Decrypt(Byte[] data)
        {
            return Crypt(data, false);
        }
        public Byte[] Encrypt(Byte[] data)
        {
            return Crypt(data, true);
        }
        public Byte[] Crypt(Byte[] data, Boolean encrypt)
        {
            var CryptedData = new Byte[data.Length];
            var CryptKey = encrypt ? EncryptionKey : DecryptionKey;
            for (int i = 0; i < data.Length / BlockSize; i++)
                DesFunc(CryptKey, data, i * BlockSize, CryptedData, i * BlockSize);
            return CryptedData;
        }
        protected UInt32[] GenerateKey(Boolean encrypt, Byte[] key)
        {
            var newKey = new UInt32[32];
            var pc1m = new Boolean[56];
            var pcr = new Boolean[56];

            for (var j = 0; j < 56; j++)
            {
                var l = PermutatedChoice1[j];
                pc1m[j] = (key[l >> 3] & (1 << (l & 7))) != 0;
            }

            for (var i = 0; i < 16; i++)
            {
                var m = (15 - i) << 1;
                if (encrypt)
                    m = i << 1;
                var n = m + 1;
                newKey[m] = newKey[n] = 0;
                var l = 0u + NumLeftRotations[i];
                for (var j = 0u; j < 28; j++)
                {
                    l = j + NumLeftRotations[i];
                    if (l < 28)
                        pcr[j] = pc1m[l];
                    else
                        pcr[j] = pc1m[l - 28];
                }

                for (var j = 28u; j < 56; j++)
                {
                    l = j + NumLeftRotations[i];
                    if (l < 56)
                        pcr[j] = pc1m[l];
                    else
                        pcr[j] = pc1m[l - 28];
                }

                for (var j = 0; j < 24; j++)
                {
                    if (pcr[PermutatedChoice2[j]])
                        newKey[m] |= 0x800000u >> j;
                    if (pcr[PermutatedChoice2[j + 24]])
                        newKey[n] |= 0x800000u >> j;
                }
            }
            for (var i = 0; i != 32; i += 2)
            {
                var i1 = newKey[i];
                var i2 = newKey[i + 1];

                newKey[i] = ((i1 & 0x00fc0000) << 6) |
                            ((i1 & 0x00000fc0) << 10) |
                            ((i2 & 0x00fc0000) >> 10) |
                            ((i2 & 0x00000fc0) >> 6);

                newKey[i + 1] = ((i1 & 0x0003f000) << 12) |
                                ((i1 & 0x0000003f) << 16) |
                                ((i2 & 0x0003f000) >> 4) |
                                (i2 & 0x0000003f);
            }
            return newKey;
        }
        protected static void DesFunc(UInt32[] wKey, Byte[] input, Int32 inOff, Byte[] outBytes, Int32 outOff)
        {
            Array.Reverse(input, inOff, 4);
            var left = BitConverter.ToUInt32(input, inOff);
            Array.Reverse(input, inOff + 4, 4);
            var right = BitConverter.ToUInt32(input, inOff + 4);

            var work = (right ^ (left >> 4)) & 0x0f0f0f0f;
            right = work ^ right;
            left = left ^ 16 * work;
            work = (right ^ (left >> 16)) & 0x0000ffff;
            right = work ^ right;
            left = left ^ (work << 16);
            work = (left ^ (right >> 2)) & 0x33333333;
            left = work ^ left;
            right = right ^ 4 * work;
            work = (left ^ (right >> 8)) & 0x00ff00ff;
            left = work ^ left;
            right = right ^ (work << 8);
            right = (right << 1) | (right >> 31);
            work = (left ^ right) & 0xaaaaaaaa;
            right = right ^ work;
            left = work ^ left;
            left = (left << 1) | (left >> 31);
            for (var round = 0; round < 8; round++)
            {
                work = (right << 28) | (right >> 4);
                work = wKey[round * 4 + 0] ^ work;
                var fval = SPBox[6, work & 0x3f];
                fval |= SPBox[4, (work >> 8) & 0x3f];
                fval |= SPBox[2, (work >> 16) & 0x3f];
                fval |= SPBox[0, (work >> 24) & 0x3f];
                work = wKey[round * 4 + 1] ^ right;
                fval |= SPBox[7, work & 0x3f];
                fval |= SPBox[5, (work >> 8) & 0x3f];
                fval |= SPBox[3, (work >> 16) & 0x3f];
                fval |= SPBox[1, (work >> 24) & 0x3f];
                left ^= fval;
                work = (left << 28) | (left >> 4);
                work = wKey[round * 4 + 2] ^ work;
                fval = SPBox[6, work & 0x3f];
                fval |= SPBox[4, (work >> 8) & 0x3f];
                fval |= SPBox[2, (work >> 16) & 0x3f];
                fval |= SPBox[0, (work >> 24) & 0x3f];
                work = wKey[round * 4 + 3] ^ left;
                fval |= SPBox[7, work & 0x3f];
                fval |= SPBox[5, (work >> 8) & 0x3f];
                fval |= SPBox[3, (work >> 16) & 0x3f];
                fval |= SPBox[1, (work >> 24) & 0x3f];
                right ^= fval;
            }

            right = (right << 31) | (right >> 1);
            work = (left ^ right) & 0xaaaaaaaa;
            right = work ^ right;
            left = work ^ left;
            left = (left << 31) | (left >> 1);
            work = (right ^ (left >> 8)) & 0x00ff00ff;
            right = work ^ right;
            left = left ^ (work << 8);
            work = (right ^ (left >> 2)) & 0x33333333;
            right = work ^ right;
            left = left ^ 4 * work;
            work = (left ^ (right >> 16)) & 0x0000ffff;
            left = work ^ left;
            right = right ^ (work << 16);
            work = (left ^ (right >> 4)) & 0x0f0f0f0f;
            left = work ^ left;
            right = right ^ 16 * work;

            Array.Copy(BitConverter.GetBytes(right), 0, outBytes, outOff, 4);
            Array.Reverse(outBytes, outOff, 4);
            Array.Copy(BitConverter.GetBytes(left), 0, outBytes, outOff + 4, 4);
            Array.Reverse(outBytes, outOff + 4, 4);
        }
    }
}
