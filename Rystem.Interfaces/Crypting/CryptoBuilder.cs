using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoBuilder : IInstallingBuilder
    {
        private readonly IConfiguration CryptoConfiguration;
        private readonly CryptoSelector CryptoSelector;
        internal CryptoBuilder(IConfiguration cryptoConfiguration, CryptoSelector cryptoSelector)
        {
            this.CryptoConfiguration = cryptoConfiguration;
            this.CryptoSelector = cryptoSelector;
        }
        public InstallerType InstallerType => InstallerType.Crypting;
        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.CryptoSelector.Builder.AddConfiguration(this.CryptoConfiguration, this.InstallerType, installation);
            return this.CryptoSelector.Builder;
        }
    }
}
