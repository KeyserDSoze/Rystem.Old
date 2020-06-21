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

        public Task<bool> AcquireAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAcquiredAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReleaseAsync(string key)
        {
            throw new NotImplementedException();
        }
    }
}