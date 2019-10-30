using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoConfiguration : IRystemConfiguration
    {
        public string Name { get; set; }
        public CryptoType Type { get; set; }
    }
}
