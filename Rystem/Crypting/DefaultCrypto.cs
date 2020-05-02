using System;

namespace Rystem.Crypting
{
    internal class DefaultCrypto : ICrypto
    {
        public string Message { get; set; }
        public string CryptedMessage { get; set; }
        static DefaultCrypto()
        {
            CryptoInstaller.Configure<DefaultCrypto>(new RjindaelConfiguration(), Installation.Default);
            CryptoInstaller.Configure<DefaultCrypto>(new Sha256Configuration(), Installation.Inst00);
        }
    }
}
