using Rystem.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal class InMemory<T> : IMultitonIntegration<T>
        where T : IMultiton, new()
    {
        private readonly ExpiringProperties ExpiringProperties;
        private readonly static ConcurrentDictionary<string, Dummy> Instances = new ConcurrentDictionary<string, Dummy>();
        internal InMemory(ExpiringProperties configuration)
            => ExpiringProperties = configuration;
        public Task<T> InstanceAsync(string key)
            => Task.FromResult(Instances[key].Instance);
        public Task<bool> UpdateAsync(string key, T value, TimeSpan expireTime)
        {
            if (value == null)
                return Task.FromResult(false);
            long multitonExpireTime = ExpiringProperties.ExpireSeconds;
            if (expireTime != default)
                multitonExpireTime = (long)expireTime.TotalSeconds;
            if (!Instances.ContainsKey(key))
                Instances.TryAdd(key, new Dummy());
            if (Instances[key].Instance == null || !ExpiringProperties.Consistency || Instances[key].Instance.ToDefaultJson() != value.ToDefaultJson())
                Instances[key].Instance = value;
            if (multitonExpireTime > (int)ExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddSeconds(multitonExpireTime).Ticks;
            return Task.FromResult(true);
        }
        public Task<MultitonStatus<T>> ExistsAsync(string key)
        {
            if (Instances.ContainsKey(key))
            {
                if (ExpiringProperties.ExpireSeconds == (int)ExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks)
                    return Task.FromResult(MultitonStatus<T>.Ok());
                else
                    Instances.TryRemove(key, out _);
            }
            return Task.FromResult(MultitonStatus<T>.NotOk());
        }

        public Task<bool> DeleteAsync(string key)
            => Task.FromResult(Instances.TryRemove(key, out _));
        public Task<IEnumerable<string>> ListAsync()
            => Task.FromResult(Instances.Keys.Select(x => x));
        private class Dummy
        {
            internal T Instance { get; set; }
            internal long ExpiringTime { get; set; }
        }
    }
}
