using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal class MultitonManager<TCache> : IMultitonManager<TCache>
        where TCache : IMultiton, new()
    {
        private readonly IMultitonIntegration<TCache> InMemory;
        private readonly bool MemoryIsActive = false;
        private readonly IMultitonIntegration<TCache> InCloud;
        private readonly bool CloudIsActive = false;
        private readonly static object TrafficLight = new object();
        public MultitonManager(MultitonProperties configuration)
        {
            if (MemoryIsActive = configuration.InMemoryProperties != null && configuration.InMemoryProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
                InMemory = new InMemory<TCache>(configuration.InMemoryProperties);
            if (CloudIsActive = configuration.InCloudProperties != null && configuration.InCloudProperties.ExpireSeconds != (int)ExpireTime.TurnOff)
            {
                if (string.IsNullOrWhiteSpace(configuration.InCloudProperties.ConnectionString))
                    throw new ArgumentException($"Value {typeof(TCache).FullName} installed for cloud without a connection string.");
                switch (configuration.InCloudProperties.CloudType)
                {
                    case InCloudType.RedisCache:
                        InCloud = new InRedisCache<TCache>(configuration.InCloudProperties);
                        break;
                    case InCloudType.TableStorage:
                        InCloud = new InTableStorage<TCache>(configuration.InCloudProperties);
                        break;
                    case InCloudType.BlobStorage:
                        InCloud = new InBlobStorage<TCache>(configuration.InCloudProperties);
                        break;
                    default:
                        throw new NotImplementedException($"CloudType not found {configuration.InCloudProperties.CloudType}");
                }
            }
        }
        private static readonly object PromiseTrafficLight = new object();
        private static readonly object RemovePromiseTrafficLight = new object();
        private static readonly object UpdatePromiseTrafficLight = new object();
        private static readonly Dictionary<string, PromisedCache> Promised = new Dictionary<string, PromisedCache>();
        private static readonly Dictionary<string, UpdatePromisedCache> UpdatePromised = new Dictionary<string, UpdatePromisedCache>();
        public async Task<TCache> InstanceAsync(IMultitonKey<TCache> key)
        {
            TCache cache = default;
            string keyString = key.ToKeyString();
            if ((MemoryIsActive && !(await InMemory.ExistsAsync(keyString).NoContext())) || (CloudIsActive && !(await InCloud.ExistsAsync(keyString).NoContext())))
            {
                if (!Promised.ContainsKey(keyString))
                    lock (PromiseTrafficLight)
                    {
                        if (!Promised.ContainsKey(keyString))
                            Promised.Add(keyString, new PromisedCache(keyString, key.FetchAsync));
                    }
                while (Promised.ContainsKey(keyString) && !Promised[keyString].IsCompleted)
                {
                    await Task.Delay(100).NoContext();
                }
                if (Promised.ContainsKey(keyString) && Promised[keyString].HasValue)
                {
                    lock (RemovePromiseTrafficLight)
                    {
                        if (Promised.ContainsKey(keyString))
                        {
                            cache = Promised[keyString].Value.Result;
                            if (MemoryIsActive)
                                InMemory.UpdateAsync(keyString, cache, default).ToResult();
                            if (CloudIsActive)
                                UpdatePromised.Add(keyString, new UpdatePromisedCache(keyString, cache, InCloud.UpdateAsync));
                            Promised.Remove(keyString);
                        }
                    }
                    while (UpdatePromised.ContainsKey(keyString) && !UpdatePromised[keyString].IsCompleted)
                    {
                        await Task.Delay(100).NoContext();
                    }
                    if (UpdatePromised.ContainsKey(keyString))
                    {
                        lock (UpdatePromiseTrafficLight)
                        {
                            if (UpdatePromised.ContainsKey(keyString))
                                UpdatePromised.Remove(keyString);
                        }
                    }
                }
            }
            if (cache != null)
                return cache;
            else
                return MemoryIsActive ? await InMemory.InstanceAsync(keyString).NoContext() : await InCloud.InstanceAsync(keyString).NoContext();
        }
        public async Task<bool> UpdateAsync(IMultitonKey<TCache> key, TCache value, TimeSpan expiringTime)
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = await key.FetchAsync().NoContext();
            bool result = false;
            if (MemoryIsActive)
                result |= await InMemory.UpdateAsync(keyString, value, expiringTime).NoContext();
            if (CloudIsActive)
                result |= await InCloud.UpdateAsync(keyString, value, expiringTime).NoContext();
            return result;
        }
        public async Task<bool> ExistsAsync(IMultiKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                return await InMemory.ExistsAsync(keyString).NoContext();
            else if (CloudIsActive)
                return await InCloud.ExistsAsync(keyString).NoContext();
            return false;
        }
        public async Task<bool> DeleteAsync(IMultiKey key)
        {
            string keyString = key.ToKeyString();
            bool result = false;
            if (MemoryIsActive)
                result |= await InMemory.DeleteAsync(keyString).NoContext();
            if (CloudIsActive)
                result |= await InCloud.DeleteAsync(keyString).NoContext();
            return result;
        }
        public async Task<IEnumerable<string>> ListAsync()
        {
            if (CloudIsActive)
                return await InCloud.ListAsync().NoContext();
            if (MemoryIsActive)
                return await InMemory.ListAsync().NoContext();
            return null;
        }
        private class PromisedCache
        {
            public string Key { get; }
            public PromisedCache(string key, Func<Task<TCache>> fetch)
            {
                this.Key = key;
                this.Value = fetch.Invoke();
            }
            public Task<TCache> Value { get; set; }
            public bool HasValue => this.Value.Status == TaskStatus.RanToCompletion;
            public bool IsCompleted => this.Value.Status == TaskStatus.RanToCompletion || this.Value.Status == TaskStatus.Faulted || this.Value.Status == TaskStatus.Canceled;
        }
        private class UpdatePromisedCache
        {
            public string Key { get; }
            public UpdatePromisedCache(string key, TCache value, Func<string, TCache, TimeSpan, Task> updater)
            {
                this.Key = key;
                this.Value = updater.Invoke(key, value, default);
            }
            public Task Value { get; set; }
            public bool IsCompleted => this.Value.Status == TaskStatus.RanToCompletion || this.Value.Status == TaskStatus.Faulted || this.Value.Status == TaskStatus.Canceled;
        }
    }
}
