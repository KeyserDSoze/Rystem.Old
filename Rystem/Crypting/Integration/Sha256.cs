using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    internal class Sha256 : ICryptoIntegration
    {
        private CryptoConfiguration CryptoConfiguration;
        public Sha256(CryptoConfiguration configuration) => this.CryptoConfiguration = CryptoConfiguration;

        public string Decrypt(string encryptedMessage)
        {
            throw new NotImplementedException();
        }

        public string Encrypt(string message)
        {
            throw new NotImplementedException();
        }
    }
}
