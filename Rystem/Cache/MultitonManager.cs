using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal delegate IMultiton CreationFunction(IMultitonKey key);
    internal class MultitonManager<T> : AMultitonManager
        where T : IMultiton
    {
        private readonly static AMultitonIntegration<T> InMemory;
        private readonly static bool MemoryIsActive = false;
        private readonly static AMultitonIntegration<T> InCloud;
        private readonly static bool CloudIsActive = false;
        private readonly static string FullName = typeof(T).FullName;
        private readonly static object TrafficLight = new object();
        private readonly static CreationFunction CreationFunction = ((IMultiton)Activator.CreateInstance(typeof(T))).Fetch;
        static MultitonManager()
        {
            MultitonInstaller.MultitonConfiguration configuration = MultitonInstaller.GetConfiguration(typeof(T));
            if (MemoryIsActive = configuration.ExpireMultiton != (int)MultitonExpireTime.TurnOff)
                InMemory = new InMemory<T>(configuration);
            if (CloudIsActive = configuration.ExpireCache != (int)CacheExpireTime.TurnOff && !string.IsNullOrWhiteSpace(configuration.ConnectionString))
            {
                if (configuration.ConnectionString.ToLower().Contains("redis.cache.windows.net"))
                    InCloud = new InRedisCache<T>(configuration);
                else
                    InCloud = new InTableStorage<T>(configuration);
            }
        }
        internal override IMultiton Get(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive && !InMemory.Exists(keyString))
            {
                lock (TrafficLight)
                {
                    if (!InMemory.Exists(keyString))
                    {
                        if (CloudIsActive && InCloud.Exists(keyString))
                            InMemory.Update(keyString, InCloud.Instance(keyString));
                        else
                            Update(key, (T)CreationFunction.Invoke(key));
                    }
                }
            }
            else if (!MemoryIsActive)
                if (!InCloud.Exists(keyString))
                    lock (TrafficLight)
                        if (!InCloud.Exists(keyString))
                            Update(key, (T)CreationFunction.Invoke(key));
            return Instance(key);
        }
        internal override IMultiton Instance(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                return InMemory.Instance(keyString);
            if (CloudIsActive)
                return InCloud.Instance(CloudKeyToString(keyString));
            throw new NotImplementedException($"Error in {FullName}. Please use MultitonInstaller in static constructor of your key");
        }
        internal override bool Update(IMultitonKey key, IMultiton value)
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = CreationFunction.Invoke(key);
            return (!MemoryIsActive || InMemory.Update(keyString, (T)value)) &&
                (!CloudIsActive || InCloud.Update(CloudKeyToString(keyString), (T)value)) && (MemoryIsActive || CloudIsActive);
        }

        internal override bool Exists(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            return (!MemoryIsActive || InMemory.Exists(keyString))
                   || (!CloudIsActive || InCloud.Exists(CloudKeyToString(keyString)))
                   && (MemoryIsActive || CloudIsActive);
        }

        internal override bool Delete(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            return (!MemoryIsActive || InMemory.Delete(keyString)) 
                   && (!CloudIsActive || InCloud.Delete(CloudKeyToString(keyString))) 
                   && (MemoryIsActive || CloudIsActive);
        }

        internal override IEnumerable<string> List()
        {
            if (CloudIsActive)
                return InCloud.List();
            if (MemoryIsActive)
                return InMemory.List();
            return null;
        }
        private static string CloudKeyToString(string keyString)
            => $"{FullName}{MultitonConst.Separator}{keyString}";
    }
}
