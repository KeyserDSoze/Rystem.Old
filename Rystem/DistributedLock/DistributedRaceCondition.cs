using Rystem;
using Rystem.DistributedLock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public class DistributedRaceCondition : ILock
    {
        private readonly int Attempt;
        public DistributedRaceCondition(string connectionString, LockType lockType, int retryAttempts = 3)
        {
            this.ConnectionString = connectionString;
            this.LockType = lockType;
            this.Attempt = retryAttempts;
        }
        public async Task<RaceConditionResponse> ExecuteAsync(Func<Task> action)
        {
            if (await this.AcquireAsync())
            {
                List<Exception> exceptions = new List<Exception>();
                _ = await Retry.Create(action, this.Attempt)
                    .CatchError(exceptions)
                    .ExecuteAsync();
                _ = await Retry.Create(async () => await this.ReleaseAsync(), this.Attempt)
                    .CatchError(exceptions)
                    .ExecuteAsync();
                return new RaceConditionResponse(true, exceptions);
            }
            while (await this.IsAcquiredAsync())
                await Task.Delay(480);
            return new RaceConditionResponse(false, default);
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