using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMEncryption
{
    public static class ROMUnlua
    {
        public static void Unlua(String file)
        {
            Console.WriteLine(file);
            var origFile = File.ReadAllBytes(file);
            if (origFile[0] != 0x2a)
                throw new Exception("2a file header not found");
            var luac = new List<Byte> { 0x1B, 0x4C, 0x75, 0x61, 0x53, 0x00, 0x19, 0x93,
                0x0D, 0x0A, 0x1A, 0x0A, 0x04, 0x04, 0x04, 0x08,
                0x08, 0x78, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x77, 0x40, 0x01, 0x00 };
            var path = Encoding.ASCII.GetString(origFile.Skip(1).Skip(0x100).TakeWhile(b => b != 0).ToArray()).Substring(1).Replace("/", @"\");
            luac.AddRange(origFile.Skip(1).Skip(0x100).SkipWhile(b=>b != 0));
            var luacFile = file.Replace(".bytes", "") + ".luac";
            File.WriteAllBytes(luacFile, luac.ToArray());
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@"java.exe", "-jar unluac_2015_06_13.jar " + luacFile);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            //p.WaitForExit();
            var lua = p.StandardOutput.ReadToEnd();
            File.WriteAllText(luacFile.Replace("luac", "lua"), lua);
            //Directory.CreateDirectory(Path.GetDirectoryName(path));
            //File.WriteAllText(path, o);
        }
    }
}
