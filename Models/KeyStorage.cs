using System.Security.Cryptography;
using System.Text;

namespace LinksApp.Models
{
    public class KeyStorage
    {
        private static RSACryptoServiceProvider? Algorithm;
        
        public static string GenerateKey(string targetFile)
        {
            Algorithm = new RSACryptoServiceProvider();

            
            string CompleteKey = Algorithm.ToXmlString(true);
            byte[] KeyBytes = Encoding.UTF8.GetBytes(CompleteKey);

            using (FileStream fs = new FileStream(targetFile, FileMode.Create))
            {
                fs.Write(KeyBytes, 0, KeyBytes.Length);
            }

            
            return Algorithm.ToXmlString(false);
        }

        public static byte[] EncryptData2(string data, string keyFile)
        {
            
            Algorithm = new RSACryptoServiceProvider();
            ReadKey(Algorithm, keyFile);

            
            return Algorithm.Encrypt(
                   Encoding.UTF8.GetBytes(data), true);
        }

        public static void ReadKey(RSACryptoServiceProvider algorithm, string keyFile)
        {
            byte[] KeyBytes;

            using (FileStream fs = new FileStream(keyFile, FileMode.Open))
            {
                KeyBytes = new byte[fs.Length];
                fs.Read(KeyBytes, 0, (int)fs.Length);
            }

            algorithm.FromXmlString(Encoding.UTF8.GetString(KeyBytes));
        }

        public static string DecryptData(byte[] data, string keyFile)
        {
            Algorithm = new RSACryptoServiceProvider();
            ReadKey(Algorithm, keyFile);

            byte[] ClearData = Algorithm.Decrypt(data, true);
            return Encoding.UTF8.GetString(ClearData);
        }

    }
}
