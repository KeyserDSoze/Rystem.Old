using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class Sha256Configuration : CryptoConfiguration
    {
        public override CryptoType Type => CryptoType.Sha256;
    }
}
