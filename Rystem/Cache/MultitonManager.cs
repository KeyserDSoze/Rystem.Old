using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal class MultitonManager<T> : IMultitonManager
        where T : IMultiton
    {
        private readonly IMultitonIntegration<T> InMemory;
        private readonly bool MemoryIsActive = false;
        private readonly IMultitonIntegration<T> InCloud;
        private readonly bool CloudIsActive = false;
        private readonly static string FullName = typeof(T).FullName;
        private readonly static object TrafficLight = new object();
        public MultitonManager(MultitonProperties configuration)
        {
            if (MemoryIsActive = configuration.InMemoryProperties != null && configuration.InMemoryProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
                InMemory = new InMemory<T>(configuration.InMemoryProperties);
            if (CloudIsActive = configuration.InCloudProperties != null && configuration.InCloudProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
            {
                if (string.IsNullOrWhiteSpace(configuration.InCloudProperties.ConnectionString))
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
        [Obsolete("This method will be removed in future version. Please use IMultitonKey<TCache> instead of IMultitonKey.")]
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
                                InMemory.Update(keyString, InCloud.Instance(keyString), default);
                            else
                                Update(key, key.Fetch(), default);
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
                            Update(key, key.Fetch(), default);
                return InCloud.Instance(keyString);
            }
        }
        public TEntry Instance<TEntry>(IMultitonKey<TEntry> key)
            where TEntry : IMultiton, new()
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
                                InMemory.Update(keyString, InCloud.Instance(keyString), default);
                            else
                                Update(key, key.Fetch(), default);
                        }
                    }
                }
                return (TEntry)(InMemory.Instance(keyString) as IMultiton);
            }
            else
            {
                if (!InCloud.Exists(keyString))
                    lock (TrafficLight)
                        if (!InCloud.Exists(keyString))
                            Update(key, key.Fetch(), default);
                return (TEntry)(InCloud.Instance(keyString) as IMultiton);
            }
        }

        [Obsolete("This method will be removed in future version. Please use IMultitonKey<TCache> instead of IMultitonKey.")]
        public bool Update(IMultitonKey key, IMultiton value, TimeSpan expiringTime)
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = key.Fetch();
            return (!MemoryIsActive || InMemory.Update(keyString, (T)value, expiringTime)) &&
                (!CloudIsActive || InCloud.Update(keyString, (T)value, expiringTime));
        }
        public bool Update<TEntry>(IMultitonKey<TEntry> key, TEntry value, TimeSpan expiringTime)
            where TEntry : IMultiton, new()
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = key.Fetch();
            return (!MemoryIsActive || InMemory.Update(keyString, (T)(value as IMultiton), expiringTime)) &&
                (!CloudIsActive || InCloud.Update(keyString, (T)(value as IMultiton), expiringTime));
        }

        public bool Exists(IMultiKey key)
        {
            string keyString = key.ToKeyString();
            return (!MemoryIsActive || InMemory.Exists(keyString))
                   && (!CloudIsActive || InCloud.Exists(keyString));
        }

        public bool Delete(IMultiKey key)
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
