using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoBuilder : IBuilder
    {
        private readonly IConfiguration CryptoConfiguration;
        private readonly CryptoSelector CryptoSelector;
        internal CryptoBuilder(IConfiguration cryptoConfiguration, CryptoSelector cryptoSelector)
        {
            this.CryptoConfiguration = cryptoConfiguration;
            this.CryptoSelector = cryptoSelector;
        }
        public InstallerType InstallerType => InstallerType.Crypting;
        public ConfigurationBuilder Build()
        {
            this.CryptoSelector.Installer.AddConfiguration(this.CryptoConfiguration, this.InstallerType);
            return this.CryptoSelector.Installer.Builder;
        }
    }
}
