using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal class LockManager<TEntity> : ILockManager<TEntity>
        where TEntity : ILock
    {
        private readonly IDictionary<Installation, ILockIntegration> Integrations = new Dictionary<Installation, ILockIntegration>();
        private readonly IDictionary<Installation, LockConfiguration> LockConfiguration;
        private static readonly object TrafficLight = new object();
        private ILockIntegration Integration(Installation installation)
        {
            if (!Integrations.ContainsKey(installation))
                lock (TrafficLight)
                    if (!Integrations.ContainsKey(installation))
                    {
                        LockConfiguration configuration = LockConfiguration[installation];
                        switch (configuration.Type)
                        {
                            case LockType.BlobStorage:
                                Integrations.Add(installation, new BlobStorageIntegration(configuration, this.Entity.GetType().FullName));
                                break;
                            case LockType.RedisCache:
                                Integrations.Add(installation, new RedisCacheIntegration(configuration, this.Entity.GetType().FullName));
                                break;
                            default:
                                throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
                        }
                    }
            return Integrations[installation];
        }
        public InstallerType InstallerType => InstallerType.Lock;
        private readonly TEntity Entity;
        public LockManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            LockConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as LockConfiguration);
            this.Entity = entity;
        }

        public bool Acquire(Installation installation = Installation.Default)
            => this.Integration(installation).Acquire();
        public bool Release(Installation installation = Installation.Default)
            => this.Integration(installation).Release();

        public bool IsAcquired(Installation installation = Installation.Default)
        => this.Integration(installation).IsAcquired();
    }
}