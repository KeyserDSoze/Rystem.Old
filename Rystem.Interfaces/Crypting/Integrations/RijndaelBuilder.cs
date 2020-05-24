using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Crypting
{
    public class RijndaelBuilder
    {
        public RjindaelConfiguration CryptoConfiguration { get; }
        public RijndaelBuilder()
            => this.CryptoConfiguration = new RjindaelConfiguration();
        public RijndaelBuilder(string passwordHash, string saltKey, string vIKey) 
            => this.CryptoConfiguration = new RjindaelConfiguration(passwordHash, saltKey, vIKey);
        public RijndaelBuilder(CipherMode cipherMode, PaddingMode paddingMode)
            => this.CryptoConfiguration = new RjindaelConfiguration(cipherMode, paddingMode);
        public RijndaelBuilder(string passwordHash, string saltKey, string vIKey, CipherMode cipherMode, PaddingMode paddingMode)
            => this.CryptoConfiguration = new RjindaelConfiguration(passwordHash, saltKey, vIKey, cipherMode, paddingMode);
    }
}
