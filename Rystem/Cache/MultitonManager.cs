using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal class MultitonManager<T> : IMultitonManager
        where T : IMultiton
    {
        private readonly AMultitonIntegration<T> InMemory;
        private readonly bool MemoryIsActive = false;
        private readonly AMultitonIntegration<T> InCloud;
        private readonly bool CloudIsActive = false;
        private readonly static string FullName = typeof(T).FullName;
        private readonly static object TrafficLight = new object();
        public MultitonManager(MultitonProperties configuration)
        {
            if (MemoryIsActive = configuration.InMemoryProperties != null && configuration.InMemoryProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
                InMemory = new InMemory<T>(configuration.InMemoryProperties);
            if (CloudIsActive = configuration.InCloudProperties != null && configuration.InCloudProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
            {
                if(string.IsNullOrWhiteSpace(configuration.InCloudProperties.ConnectionString))
                    throw new ArgumentException($"Value {typeof(T).FullName} installed for cloud without a connection string.");
                switch (configuration.InCloudProperties.CloudType)
                {
                    case InCloudType.RedisCache:
                        InCloud = new InRedisCache<T>(configuration.InCloudProperties);
                        break;
                    case InCloudType.TableStorage:
                        InCloud = new InTableStorage<T>(configuration.InCloudProperties);
                        break;
                    case InCloudType.BlobStorage:
                        InCloud = new InBlobStorage<T>(configuration.InCloudProperties);
                        break;
                    default:
                        throw new NotImplementedException($"CloudType not found {configuration.InCloudProperties.CloudType}");
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
