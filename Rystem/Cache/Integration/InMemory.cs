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
        internal InMemory(MultitonInstaller.MultitonConfiguration configuration)
        {
            ExpireMultiton = configuration.ExpireMultiton;
        }
        internal override T Instance(string key) => Instances[key].Instance;
        internal override bool Update(string key, T value)
        {
            if (!Instances.ContainsKey(key))
                Instances.Add(key, new Dummy());
            Instances[key].Instance = value;
            if (ExpireMultiton > (int)MultitonExpireTime.Infinite)
                Instances[key].ExpiringTime = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks;
            return true;
        }
        internal override bool Exists(string key)
        {
            return Instances.ContainsKey(key) && (ExpireMultiton == (int)MultitonExpireTime.Infinite || (Instances[key]?.ExpiringTime ?? 0) >= DateTime.UtcNow.Ticks);
        }
        internal override bool Delete(string key) => Instances.Remove(key);
        internal override IEnumerable<string> List() => Instances.Keys;
        private class Dummy
        {
            internal T Instance { get; set; }
            internal long ExpiringTime { get; set; }
        }
    }
}
