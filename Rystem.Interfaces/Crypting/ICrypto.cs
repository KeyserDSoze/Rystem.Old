using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public interface ICrypto
    {
        string Message { get; set; }
        string CryptedMessage { get; set; }
    }
}
