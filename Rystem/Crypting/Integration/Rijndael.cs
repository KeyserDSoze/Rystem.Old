using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Crypting
{
    internal class Rijndael : ICryptoIntegration
    {
        private readonly RjindaelConfiguration CryptoConfiguration;
        public Rijndael(RjindaelConfiguration configuration)
            => this.CryptoConfiguration = configuration;
        public string Decrypt(string encryptedMessage)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedMessage);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, this.CryptoConfiguration.Decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
        public string Encrypt(string message)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(message);
            byte[] cipherTextBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, this.CryptoConfiguration.Encryptor, CryptoStreamMode.Write))
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
    }
}
