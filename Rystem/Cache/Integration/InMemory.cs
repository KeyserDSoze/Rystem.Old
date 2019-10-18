using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal class InMemory<T> : AMultitonIntegration<T>
        where T : IMultiton
    {
        private static int ExpireMultiton = 0;
        private static Dictionary<string, Dummy> Instances = new Dictionary<string, Dummy>();
        internal InMemory(ExpiringProperties configuration) 
            => ExpireMultiton = configuration.ExpireSeconds;
        internal override T Instance(string key) 
            => Instances[key].Instance;
        internal override bool Update(string key, T value)
        {
            if (!Instances.ContainsKey(key))
                Instances.Add(key, new Dummy());
            Instances[key].Instance = value;
            if (ExpireMultiton > (int)ExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddSeconds(ExpireMultiton).Ticks;
            return true;
        }
        internal override bool Exists(string key) 
            => Instances.ContainsKey(key) && (ExpireMultiton == (int)ExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks);
        internal override bool Delete(string key) 
            => Instances.Remove(key);
        internal override IEnumerable<string> List() 
            => Instances.Keys;
        private class Dummy
        {
            internal T Instance { get; set; }
            internal long ExpiringTime { get; set; }
        }
    }
}
