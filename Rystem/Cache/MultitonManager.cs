using Rystem.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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

        private static readonly ConcurrentDictionary<string, Lazy<PromisedCache>> Promised = new ConcurrentDictionary<string, Lazy<PromisedCache>>();
        private static readonly ConcurrentDictionary<string, Lazy<UpdatePromisedCache>> UpdatePromised = new ConcurrentDictionary<string, Lazy<UpdatePromisedCache>>();

        public async Task<TCache> InstanceAsync(IMultitonKey<TCache> key)
        {
            TCache cache = default;
            string keyString = key.ToKeyString();
            bool cloudChecked = false;
            if ((MemoryIsActive && !(await InMemory.ExistsAsync(keyString).NoContext())) || ((cloudChecked = true) && CloudIsActive && !(await InCloud.ExistsAsync(keyString).NoContext())))
            {
                Instancer instance = new Instancer(key, keyString, cloudChecked, InMemory, InCloud);
                Promised.TryAdd(keyString,
                    new Lazy<PromisedCache>(() => new PromisedCache(instance,
                    instance.GetValueAsync
                ), LazyThreadSafetyMode.None));
                while (Promised.ContainsKey(keyString) && !Promised[keyString].Value.IsCompleted)
                    await Task.Delay(200).NoContext();
                UpdatePromised.TryAdd(keyString,
                    new Lazy<UpdatePromisedCache>(() => new UpdatePromisedCache(Promised[keyString].Value),
                    LazyThreadSafetyMode.None));
                while (UpdatePromised.ContainsKey(keyString) && !UpdatePromised[keyString].Value.IsCompleted)
                {
                    if (cache == null && Promised.ContainsKey(keyString))
                        cache = Promised[keyString].Value.Instance.Result.CachedData;
                    await Task.Delay(200).NoContext();
                }
                if (UpdatePromised.TryRemove(keyString, out Lazy<UpdatePromisedCache> entity))
                    cache = entity.Value.PromisedCache.Instance.Result.CachedData;
                Promised.TryRemove(keyString, out _);
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
        private class Instancer
        {
            public IMultitonKey<TCache> Key { get; }
            public string KeyString { get; }
            public bool CloudIsActive { get; }
            public bool CloudChecked { get; }
            public bool MemoryIsActive { get; }
            public IMultitonIntegration<TCache> InMemory { get; }
            public IMultitonIntegration<TCache> InCloud { get; }
            public FetcherResult Result { get; private set; }
            public Instancer(IMultitonKey<TCache> key, string keyString, bool cloudChecked, IMultitonIntegration<TCache> inMemory, IMultitonIntegration<TCache> inCloud)
            {
                this.Key = key;
                this.KeyString = keyString;
                this.InMemory = inMemory;
                this.InCloud = inCloud;
                this.CloudIsActive = inCloud != null;
                this.MemoryIsActive = inMemory != null;
                this.CloudChecked = cloudChecked;
            }
            public async Task<FetcherResult> GetValueAsync()
            {
                if (CloudIsActive && !CloudChecked && await InCloud.ExistsAsync(KeyString))
                    return this.Result = new FetcherResult(await InCloud.InstanceAsync(KeyString), true);
                else
                    return this.Result = new FetcherResult(await Key.FetchAsync(), false);
            }
            public async Task<FetcherResult> UpdateValueAsync()
            {
                if (MemoryIsActive)
                    await InMemory.UpdateAsync(KeyString, this.Result.CachedData, default);
                if (CloudIsActive && !this.Result.FromCloud)
                    await InCloud.UpdateAsync(KeyString, this.Result.CachedData, default);
                return this.Result;
            }
        }
        private class FetcherResult
        {
            public TCache CachedData { get; }
            public bool FromCloud { get; }
            public FetcherResult(TCache value, bool fromCloud)
            {
                this.CachedData = value;
                this.FromCloud = fromCloud;
            }
        }
        private class PromisedCache
        {
            public Instancer Instance { get; }
            public Task<FetcherResult> Value { get; }
            public PromisedCache(Instancer instance, Func<Task<FetcherResult>> fetch)
            {
                this.Instance = instance;
                this.Value = fetch.Invoke();
            }
            public bool HasValue => this.Value.Status == TaskStatus.RanToCompletion;
            public bool IsCompleted => this.Value.Status == TaskStatus.RanToCompletion || this.Value.Status == TaskStatus.Faulted || this.Value.Status == TaskStatus.Canceled;
        }
        private class UpdatePromisedCache
        {
            public PromisedCache PromisedCache { get; }
            public Task Value { get; }
            public UpdatePromisedCache(PromisedCache promisedCache)
            {
                this.PromisedCache = promisedCache;
                this.Value = promisedCache.Instance.UpdateValueAsync();
            }
            public bool IsCompleted => this.Value.Status == TaskStatus.RanToCompletion || this.Value.Status == TaskStatus.Faulted || this.Value.Status == TaskStatus.Canceled;
        }
    }
}
