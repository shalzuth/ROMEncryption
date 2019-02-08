namespace ROMEncryption
{
    class Program
    {
        static void Main(string[] args)
        {
            ROMUnityXor.DecryptFile(@"com.gravity.romg_1.0.3-308\assets\bin\Data\Managed\Assembly-CSharp-firstpass.dll");
            ROMUnityXor.DecryptFile(@"com.gravity.romg_1.0.3-308\assets\bin\Data\Managed\Assembly-CSharp.dll");

            //ROMDesCipher.DecryptFile("CSharpObjectForLogin.bytes");
            //ROMDesCipher.EncryptFile("CSharpObjectForLogin.lua");
        }
    }
}
