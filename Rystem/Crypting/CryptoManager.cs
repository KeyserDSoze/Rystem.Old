using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
                        Integrations.Add(configuration.Key, new Rijndael(configuration.Value));
                        break;
                    case CryptoType.Sha256:
                        Integrations.Add(configuration.Key, new Sha256(configuration.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }

        public TInnerEntity Decrypt<TInnerEntity>(TInnerEntity entity, Installation installation) where TInnerEntity : ICrypto
        {
            entity.Message = this.Integrations[installation].Decrypt(entity.Message);
            return entity;
        }

        public TInnerEntity Encrypt<TInnerEntity>(TInnerEntity entity, Installation installation) where TInnerEntity : ICrypto
        {
            entity.Message = this.Integrations[installation].Encrypt(entity.Message);
            return entity;
        }
    }
}
