using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal delegate IMultiton CreationFunction(IMultitonKey key);
    internal class MultitonManager<T> : IMultitonManager
        where T : IMultiton
    {
        private readonly static AMultitonIntegration<T> InMemory;
        private readonly static bool MemoryIsActive = false;
        private readonly static AMultitonIntegration<T> InCloud;
        private readonly static bool CloudIsActive = false;
        private readonly static string FullName = typeof(T).FullName;
        private readonly static object TrafficLight = new object();
        static MultitonManager()
        {
            MultitonInstaller.MultitonConfiguration configuration = MultitonInstaller.GetConfiguration(typeof(T));
            if (MemoryIsActive = configuration.ExpireMultiton != (int)MultitonExpireTime.TurnOff)
                InMemory = new InMemory<T>(configuration);
            if (CloudIsActive = configuration.ExpireCache != (int)CacheExpireTime.TurnOff && !string.IsNullOrWhiteSpace(configuration.ConnectionString))
            {
                switch (configuration.InCloudType)
                {
                    case InCloudType.RedisCache:
                        InCloud = new InRedisCache<T>(configuration);
                        break;
                    case InCloudType.TableStorage:
                        InCloud = new InTableStorage<T>(configuration);
                        break;
                    case InCloudType.BlobStorage:
                        InCloud = new InBlobStorage<T>(configuration);
                        break;
                    default:
                        throw new NotImplementedException($"InCloudType not found {configuration.InCloudType}");
                }
            }
        }
        public IMultiton Instance(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
            {
                if (!InMemory.Exists(keyString))
                {
                    lock (TrafficLight)
                    {
                        if (!InMemory.Exists(keyString))
                        {
                            if (CloudIsActive && InCloud.Exists(keyString))
                                InMemory.Update(keyString, InCloud.Instance(keyString));
                            else
                                Update(key, key.Fetch());
                        }
                    }
                }
                return InMemory.Instance(keyString);
            }
            else
            {
                if (!InCloud.Exists(keyString))
                    lock (TrafficLight)
                        if (!InCloud.Exists(keyString))
                            Update(key, key.Fetch());
                return InCloud.Instance(keyString);
            }
        }
        public bool Update(IMultitonKey key, IMultiton value)
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = key.Fetch();
            return (!MemoryIsActive || InMemory.Update(keyString, (T)value)) &&
                (!CloudIsActive || InCloud.Update(keyString, (T)value));
        }

        public bool Exists(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            return (!MemoryIsActive || InMemory.Exists(keyString))
                   && (!CloudIsActive || InCloud.Exists(keyString));
        }

        public bool Delete(IMultitonKey key)
        {
            string keyString = key.ToKeyString();
            return (!MemoryIsActive || InMemory.Delete(keyString))
                   && (!CloudIsActive || InCloud.Delete(keyString));
        }

        public IEnumerable<string> List()
        {
            if (CloudIsActive)
                return InCloud.List();
            if (MemoryIsActive)
                return InMemory.List();
            return null;
        }
       
    }
}
