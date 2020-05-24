using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoBuilder
    {
        private readonly IConfiguration CryptoConfiguration;
        private readonly CryptoSelector CryptoSelector;
        public CryptoBuilder(IConfiguration cryptoConfiguration, CryptoSelector cryptoSelector)
        {
            this.CryptoConfiguration = cryptoConfiguration;
            this.CryptoSelector = cryptoSelector;
        }
        public ConfigurationBuilder Build()
        {
            this.CryptoSelector.Installer.AddConfiguration(this.CryptoConfiguration);
            return this.CryptoSelector.Installer.Builder;
        }
    }
}
