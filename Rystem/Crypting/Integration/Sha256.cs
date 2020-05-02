using System;
using System.Text;
using System.Security.Cryptography;

namespace Rystem.Crypting
{
    internal class Sha256 : ICryptoIntegration
    {
        private readonly Sha256Configuration CryptoConfiguration;
        public Sha256(Sha256Configuration configuration)
            => this.CryptoConfiguration = configuration;

        public string Decrypt(string encryptedMessage)
            => throw new NotImplementedException("Hash is not decryptable.");

        public string Encrypt(string message)
        {
            using (SHA256 mySHA256 = SHA256.Create(this.CryptoConfiguration.GetHashName()))
            {
                byte[] bytes = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(message));
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var @byte in bytes)
                    stringBuilder.Append(@byte.ToString("x2"));
                return stringBuilder.ToString();
            }
        }
    }
}
