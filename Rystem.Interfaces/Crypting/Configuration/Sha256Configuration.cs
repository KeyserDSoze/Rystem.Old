using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class Sha256Configuration : CryptoConfiguration
    {
        public override CryptoType Type => CryptoType.Sha256;
        public HashType HashType { get; set; }
        public string GetHashName()
        {
            switch (this.HashType)
            {
                default:
                case HashType.Sha256:
                    return "SHA256";
                case HashType.SHA256Cng:
                    return "System.Security.Cryptography.SHA256Cng";
                case HashType.SHA256CryptoServiceProvider:
                    return "System.Security.Cryptography.SHA256CryptoServiceProvider";
                case HashType.Sha_256:
                    return "SHA-256";
                case HashType.System_Security_Cryptography_SHA256:
                    return "System.Security.Cryptography.SHA256";
            }
        }
    }
}
