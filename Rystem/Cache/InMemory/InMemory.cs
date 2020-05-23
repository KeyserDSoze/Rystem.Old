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
    {
        private readonly CacheProperties Properties;
        private readonly static ConcurrentDictionary<string, IInMemoryInstance> Instances = new ConcurrentDictionary<string, IInMemoryInstance>();
        private InMemoryInstance<T> MemoryInstance(string key)
            => Instances[key] as InMemoryInstance<T>;
        internal InMemory(RystemCacheProperty configuration)
        {
            Properties = configuration.MemoryProperties;
            if (Properties.GarbageCollection)
                GarbageCollector.Instance.AddDictionary(Instances);
        }

        public T Instance(string key)
            => MemoryInstance(key).Instance;
        public bool Update(string key, T value, TimeSpan expireTime)
        {
            if (value == null)
                return false;
            long multitonExpireTime = Properties.ExpireSeconds;
            if (expireTime != default)
                multitonExpireTime = (long)expireTime.TotalSeconds;
            if (!Instances.ContainsKey(key))
                Instances.TryAdd(key, new InMemoryInstance<T>());
            if (MemoryInstance(key).Instance == null || !Properties.Consistency || MemoryInstance(key).Instance.ToDefaultJson() != value.ToDefaultJson())
                MemoryInstance(key).Instance = value;
            if (multitonExpireTime > (int)ExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddSeconds(multitonExpireTime).Ticks;
            else
                Instances[key].ExpiringTime = DateTime.MaxValue.Ticks;
            return true;
        }
        public MultitonStatus<T> Exists(string key)
        {
            if (Instances.ContainsKey(key))
            {
                if (Properties.ExpireSeconds == (int)ExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks)
                    return MultitonStatus<T>.Ok();
                else
                    Instances.TryRemove(key, out _);
            }
            return MultitonStatus<T>.NotOk();
        }

        public bool Delete(string key)
            => Instances.TryRemove(key, out _);
        public IEnumerable<string> List()
            => Instances.Keys.Select(x => x);
    }
}
