using Rystem.Azure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public interface ICrypto : IConfigurator
    {
        string Message { get; set; }
        string CryptedMessage { get; set; }
    }
}
