using System;

namespace Rystem.Crypting
{
    internal class DefaultCrypto : ICrypto
    {
        public string Message { get; set; }
        public string CryptedMessage { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithCrypting().WithAes(new RijndaelBuilder()).Build()
                .WithCrypting().WithSha256(new Sha256Builder()).Build(Installation.Inst00);
        }
    }
}
