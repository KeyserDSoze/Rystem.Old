using System;
using System.Collections.Generic;

namespace Rystem.Crypting
{
    internal class CryptoManager<TEntity> : ICryptoManager
        where TEntity : ICrypto
    {
        private readonly IDictionary<Installation, ICryptoIntegration> Integrations;
        private readonly IDictionary<Installation, CryptoConfiguration> CryptoConfiguration;
        public CryptoManager()
        {
            Integrations = new Dictionary<Installation, ICryptoIntegration>();
            CryptoConfiguration = CryptoInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, CryptoConfiguration> configuration in CryptoConfiguration)
                switch (configuration.Value.Type)
                {
                    case CryptoType.Rijndael:
                        Integrations.Add(configuration.Key, new Rijndael(configuration.Value as RjindaelConfiguration));
                        break;
                    case CryptoType.Sha256:
                        Integrations.Add(configuration.Key, new Sha256(configuration.Value as Sha256Configuration));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }

        public TInnerEntity Decrypt<TInnerEntity>(TInnerEntity entity, Installation installation)
            where TInnerEntity : ICrypto
        {
            if (entity.CryptedMessage == null)
                throw new ArgumentNullException($"Property {nameof(entity.CryptedMessage)} is null.");
            entity.Message = this.Integrations[installation].Decrypt(entity.CryptedMessage);
            return entity;
        }

        public TInnerEntity Encrypt<TInnerEntity>(TInnerEntity entity, Installation installation)
            where TInnerEntity : ICrypto
        {
            if (entity.Message == null)
                throw new ArgumentNullException($"Property {nameof(entity.Message)} is null.");
            entity.CryptedMessage = this.Integrations[installation].Encrypt(entity.Message);
            return entity;
        }
    }
}
