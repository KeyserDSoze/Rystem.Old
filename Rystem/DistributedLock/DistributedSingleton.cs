using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    public class DistributedSingleton : ILock
    {
        public DistributedSingleton(string connectionString, LockType lockType)
        {
            this.ConnectionString = connectionString;
            this.LockType = lockType;
        }
        public async Task ExecuteAsync(Func<Task> action)
        {
            if (this.Acquire())
            {
                try
                {
                    await action.Invoke();
                }
                catch { }
                this.Release();
            }
            while (this.IsAcquired())
                await Task.Delay(480);
        }
        private readonly string ConnectionString;
        private readonly LockType LockType;
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            if (LockType == LockType.BlobStorage)
                return new ConfigurationBuilder()
                    .WithLock(this.ConnectionString)
                    .WithBlobStorage(new BlobBuilder("singleton"))
                    .Build();
            else
                return new ConfigurationBuilder()
                .WithLock(this.ConnectionString)
                .WithRedisCache(new RedisCacheBuilder("singleton"))
                .Build();
        }
    }
}