using System;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal class RedisCacheIntegration : ILockIntegration
    {
        private readonly LockConfiguration LockConfiguration;
        private readonly string Name;
        public RedisCacheIntegration(LockConfiguration lockConfiguration, string name)
        {
            this.LockConfiguration = lockConfiguration;
            this.Name = name;
        }

        public bool Acquire()
        {
            throw new NotImplementedException();
        }

        public bool IsAcquired()
        {
            throw new NotImplementedException();
        }

        public bool Release()
        {
            throw new NotImplementedException();
        }
    }
}