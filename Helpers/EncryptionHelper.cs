using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace NCMIS.Helpers
{
    public class EncryptionHelper
    {
        public static string encryption(string value)
        {
            string encryptedkey = "";
            string EncryptionKey = "CFRC2SPBNI99212"; //"MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(value);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptedkey = Convert.ToBase64String(ms.ToArray());
                }
            }

            return encryptedkey;
        }


        public static string dycryption(string cipherText)
        {
            string EncryptionKey = "CFRC2SPBNI99212";

            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }


            return cipherText;
        }



        #region Query string encryption
        private static readonly string aesKey = "CFRC2SPBNI99212"; // must be 16+ chars
        private static readonly byte[] aesSalt = new byte[] {
        0x49, 0x76, 0x61, 0x6e, 0x4d, 0x65, 0x64, 0x76,
        0x65, 0x64, 0x65, 0x76
    };

        // AES Encrypt
        public static string EncryptQueryValue(string plainText)
        {
            using var aes = Aes.Create();
            var keyGen = new Rfc2898DeriveBytes(aesKey, aesSalt);
            aes.Key = keyGen.GetBytes(32);
            aes.IV = new byte[16]; // static IV for deterministic output

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }

        // AES Decrypt
        public static string DecryptQueryValue(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            var keyGen = new Rfc2898DeriveBytes(aesKey, aesSalt);
            aes.Key = keyGen.GetBytes(32);
            aes.IV = new byte[16];

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        #endregion
    }
}


