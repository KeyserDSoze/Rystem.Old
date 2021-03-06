using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Crypting
{
    public class CryptoSelector : IBuildingSelector
    {
        public ConfigurationBuilder Builder { get; }
        internal CryptoSelector(ConfigurationBuilder builder)
            => this.Builder = builder;

        public CryptoBuilder WithAes(RijndaelBuilder rijndaelBuilder = default)
            => new CryptoBuilder((rijndaelBuilder ?? new RijndaelBuilder()).CryptoConfiguration, this);
        public CryptoBuilder WithSha256(Sha256Builder sha256Builder = default)
            => new CryptoBuilder((sha256Builder ?? new Sha256Builder()).CryptoConfiguration, this);
    }
}
