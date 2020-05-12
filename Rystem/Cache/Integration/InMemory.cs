using Rystem.Cache;
using System;
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
        private readonly static Dictionary<string, Dummy> Instances = new Dictionary<string, Dummy>();
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
                Instances.Add(key, new Dummy());
            if (Instances[key].Instance == null || !ExpiringProperties.Consistency || Instances[key].Instance.ToDefaultJson() != value.ToDefaultJson())
                Instances[key].Instance = value;
            if (multitonExpireTime > (int)ExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddSeconds(multitonExpireTime).Ticks;
            return Task.FromResult(true);
        }
        public Task<bool> ExistsAsync(string key)
        {
            if (Instances.ContainsKey(key))
            {
                if (ExpiringProperties.ExpireSeconds == (int)ExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks)
                    return Task.FromResult(true);
                else
                    Instances.Remove(key);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAsync(string key)
            => Task.FromResult(Instances.Remove(key));
        public Task<IEnumerable<string>> ListAsync()
            => Task.FromResult(Instances.Keys.Select(x => x));
        private class Dummy
        {
            internal T Instance { get; set; }
            internal long ExpiringTime { get; set; }
        }
    }
}
