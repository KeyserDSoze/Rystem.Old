using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Crypting
{
    public class Sha256Builder
    {
        public Sha256Configuration CryptoConfiguration { get; }
        public Sha256Builder(HashType hashType = HashType.Sha256)
            => this.CryptoConfiguration = new Sha256Configuration() { HashType = hashType };
    }
}
