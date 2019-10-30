using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Utility.Crypting
{
    public static class Aes
    {
        private static string PasswordHash = "A9@#d56_";
        private static string SaltKey = "7§hg!8@ò";
        private static string VIKey = "01tyç°@#78gh_jzx";
        private static CipherMode CipherMode = CipherMode.CBC;
        private static PaddingMode PaddingMode = PaddingMode.ISO10126;
        private static byte[] SaltKeyAsBytes => Encoding.ASCII.GetBytes(SaltKey);
        private static byte[] VIKeyAsBytes => Encoding.ASCII.GetBytes(VIKey);
        private static byte[] KeyAsBytes => new Rfc2898DeriveBytes(PasswordHash, SaltKeyAsBytes).GetBytes(256 / 8);
        private static RijndaelManaged SymmetricKey => new RijndaelManaged() { Mode = CipherMode, Padding = PaddingMode };
        public static void Install(string password, string saltKey, string vIKey, CipherMode cipherMode, PaddingMode paddingMode)
        {
            PasswordHash = password;
            SaltKey = saltKey;
            VIKey = vIKey;
            CipherMode = cipherMode;
            PaddingMode = paddingMode;
        }
        public static void Install(CipherMode cipherMode, PaddingMode paddingMode)
        {
            CipherMode = cipherMode;
            PaddingMode = paddingMode;
        }
        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptor = SymmetricKey.CreateEncryptor(KeyAsBytes, VIKeyAsBytes);
            byte[] cipherTextBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            var decryptor = SymmetricKey.CreateDecryptor(KeyAsBytes, VIKeyAsBytes);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}
