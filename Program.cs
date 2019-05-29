using System;
using System.IO;
using System.Collections.Generic;
namespace ROMEncryption
{
    class Program
    {
        static void DumpFromUnity3d()
        {
            var files = Directory.EnumerateFiles(@"script2");
            foreach (var file in files)
            {
                UtinyRipper.GameStructure gs = new UtinyRipper.GameStructure();
                gs.Load(new List<String> { file });
                gs.Export(file.Replace(".unity3d", ""), (a) => { return true; });
                Directory.CreateDirectory(@"rawlua\" + Path.GetFileNameWithoutExtension(file));
                var innerfiles = Directory.EnumerateFiles(file.Replace(".unity3d", "") + @"\Assets\TextAsset\", "*.bytes");
                foreach (var innerFile in innerfiles)
                    File.Copy(innerFile, @"rawlua\" + Path.GetFileNameWithoutExtension(file) + @"\" + Path.GetFileName(innerFile), true);
                Directory.Delete(file.Replace(".unity3d", ""), true);
            }
        }
        static void Main(string[] args)
        {
            //DumpFromUnity3d(); // takes about 40 seconds.

            // converting all files takes about 10 minutes.
            var files = Directory.EnumerateFiles(@"rawlua", "*.bytes", SearchOption.AllDirectories);
            foreach (var file in files)
                ROMUnlua.Unlua(file);

            ROMUnityXor.DecryptFile(@"com.gravity.romg_1.0.3-308\assets\bin\Data\Managed\Assembly-CSharp-firstpass.dll");
            ROMUnityXor.DecryptFile(@"com.gravity.romg_1.0.3-308\assets\bin\Data\Managed\Assembly-CSharp.dll");

            //ROMDesCipher.DecryptFile("CSharpObjectForLogin.bytes");
            //ROMDesCipher.EncryptFile("CSharpObjectForLogin.lua");
        }
    }
}
