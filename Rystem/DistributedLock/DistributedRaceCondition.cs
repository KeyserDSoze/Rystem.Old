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
        private readonly string Key;
        public DistributedRaceCondition(string connectionString, LockType lockType, string key, int retryAttempts = 3)
        {
            this.ConnectionString = connectionString;
            this.LockType = lockType;
            this.Attempt = retryAttempts;
            this.Key = key;
        }
        public async Task<RaceConditionResponse> ExecuteAsync(Func<Task> action, string key = null)
        {
            if (await this.AcquireAsync(key))
            {
                List<Exception> exceptions = new List<Exception>();
                _ = await Retry.Create(action, this.Attempt)
                    .CatchError(exceptions)
                    .NotThrowExceptionAfterLastAttempt()
                    .ExecuteAsync()
                    .NoContext();
                _ = await Retry.Create(async () => await this.ReleaseAsync(key).NoContext(), this.Attempt)
                    .CatchError(exceptions)
                    .NotThrowExceptionAfterLastAttempt()
                    .ExecuteAsync()
                    .NoContext();
                return new RaceConditionResponse(true, exceptions);
            }
            while (await this.IsAcquiredAsync())
                await Task.Delay(480).NoContext();
            return new RaceConditionResponse(false, default);
        }
        private readonly string ConnectionString;
        private readonly LockType LockType;
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            if (LockType == LockType.BlobStorage)
                return new ConfigurationBuilder()
                    .WithLock(this.ConnectionString)
                    .WithBlobStorage(new BlobBuilder("singleton", this.Key))
                    .Build();
            else
                return new ConfigurationBuilder()
                .WithLock(this.ConnectionString)
                .WithRedisCache(new RedisCacheBuilder("singleton", this.Key))
                .Build();
        }
    }
}