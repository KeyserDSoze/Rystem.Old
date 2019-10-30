using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoInstaller
    {
        public static void ConfigureAsDefault(CryptoConfiguration configuration)
           => Installer<CryptoConfiguration>.ConfigureAsDefault(configuration);
        public static void Configure<Entity>(CryptoConfiguration configuration, Installation installation = Installation.Default)
            where Entity : ICrypto
            => Installer<CryptoConfiguration, Entity>.Configure(configuration, installation);
        public static IDictionary<Installation, CryptoConfiguration> GetConfiguration<Entity>()
            where Entity : ICrypto
            => Installer<CryptoConfiguration, Entity>.GetConfiguration();
    }
}
