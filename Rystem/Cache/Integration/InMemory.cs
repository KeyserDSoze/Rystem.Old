using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal class InMemory<T> : IMultitonIntegration<T>
        where T : IMultiton, new()
    {
        private readonly int ExpireMultiton = 0;
        private readonly static Dictionary<string, Dummy> Instances = new Dictionary<string, Dummy>();
        internal InMemory(ExpiringProperties configuration)
            => ExpireMultiton = configuration.ExpireSeconds;
        public T Instance(string key)
            => Instances[key].Instance;
        public bool Update(string key, T value, TimeSpan expireTime)
        {
            long multitonExpireTime = ExpireMultiton;
            if (expireTime != default)
                multitonExpireTime = (long)expireTime.TotalSeconds;
            if (!Instances.ContainsKey(key))
                Instances.Add(key, new Dummy());
            Instances[key].Instance = value;
            if (multitonExpireTime > (int)ExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddSeconds(multitonExpireTime).Ticks;
            return true;
        }
        public bool Exists(string key)
        {
            if (Instances.ContainsKey(key))
            {
                if (ExpireMultiton == (int)ExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks)
                    return true;
                else
                    Instances.Remove(key);
            }
            return false;
        }

        public bool Delete(string key)
            => Instances.Remove(key);
        public IEnumerable<string> List()
            => Instances.Keys;
        private class Dummy
        {
            internal T Instance { get; set; }
            internal long ExpiringTime { get; set; }
        }
    }
}
