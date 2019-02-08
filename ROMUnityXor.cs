using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ROMEncryption
{
    public static class ROMUnityXor
    {
        public static Byte[] ROMXorKey = new Byte[] { 0xde, 0xff, 0xdc, 0x60, 0xfe, 0xfe, 0xdf, 0xff };
        public static void XorChain(Byte[] input, Byte[] xorChain)
        {
            for (int i = 0; i < input.Length; i++)
            {
                var xorChainOffset = i % xorChain.Length;
                input[i] = (Byte)(input[i] ^ xorChain[xorChainOffset]);
            }
        }
        public static void FixHeader(Byte[] fileBytes)
        {
            // not sure, correct header? still partially encrypted, but hardcoding this hack...
            fileBytes[0] = 0x4d;
            fileBytes[1] = 0x5a;
            fileBytes[2] = 0x90;
            fileBytes[3] = 0;
            fileBytes[4] = 3;
            fileBytes[5] = 0;

            var sigIndex = 0;
            for (int i = 0; i < fileBytes.Length - 4; i++)
            {
                if (Encoding.ASCII.GetString(fileBytes, i, 4) == "BSJB")
                {
                    sigIndex = i;
                    break;
                }
            }
            var cor20Header = BitConverter.ToUInt32(fileBytes, 0x18c);
            var virtualAddress = BitConverter.ToUInt32(fileBytes, 0x184);
            var metadataAddr = sigIndex + virtualAddress - cor20Header;
            var metadataAddrBytes = BitConverter.GetBytes(metadataAddr);
            var oldMetadataAddr = BitConverter.ToUInt32(fileBytes, (int)cor20Header + 0x10);
            Array.Copy(metadataAddrBytes, 0, fileBytes, cor20Header + 0x10, 4);
        }
        public static void DecryptFile(String file)
        {
            var fileBytes = File.ReadAllBytes(file).Skip(0x10).ToArray();
            XorChain(fileBytes, ROMXorKey);
            FixHeader(fileBytes);
            File.WriteAllBytes(file.Replace(".dll", ".fixed.dll"), fileBytes);
        }
    }
}
