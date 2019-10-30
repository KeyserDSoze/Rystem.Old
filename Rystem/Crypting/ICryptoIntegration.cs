using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Crypting
{
    internal interface ICryptoIntegration
    {
        string Encrypt(string message);
        string Decrypt(string encryptedMessage);
    }
}
