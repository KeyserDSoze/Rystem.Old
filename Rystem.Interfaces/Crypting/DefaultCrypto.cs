using System;

namespace Rystem.Crypting
{
    internal class DefaultCrypto : ICrypto
    {
        public string Message { get; set; }
        public string CryptedMessage { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithInstallation().WithCrypting().WithAes(new RijndaelBuilder()).Build()
                .WithInstallation(Installation.Inst00).WithCrypting().WithSha256(new Sha256Builder()).Build();
        }
    }
}
