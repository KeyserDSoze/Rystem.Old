using Rystem.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static readonly ConcurrentDictionary<string, MyLazy<PromisedCache>> Promised = new ConcurrentDictionary<string, MyLazy<PromisedCache>>();
        private class MyLazy<TEntity>
        {
            private readonly Func<TEntity> Creator;
            private TEntity value;
            private static readonly object TrafficLight = new object();
            public TEntity Value
            {
                get
                {
                    if (value == null)
                        lock (TrafficLight)
                            if (value == null)
                                return value = Creator.Invoke();
                    return value;
                }
            }

            public MyLazy(Func<TEntity> creator)
            {
                this.Creator = creator;
            }
        }
        public async Task<TCache> InstanceAsync(IMultitonKey<TCache> key)
        {
            TCache cache = default;
            string keyString = key.ToKeyString();
            bool cloudChecked = false;
            if ((MemoryIsActive && !(await InMemory.ExistsAsync(keyString).NoContext()).IsOk) || !await ExistsInCloud())
            {
                Promised.TryAdd(keyString,
                    new MyLazy<PromisedCache>(() => new PromisedCache(new Instancer(key, keyString, cloudChecked, InCloud))));
                Promised.TryGetValue(keyString, out MyLazy<PromisedCache> lazy);
                if (lazy != null)
                {
                    PromisedCache promisedCache = lazy.Value;
                    PromisedState promisedState = default;
                    while (!(promisedState = promisedCache.Run()).IsCompleted())
                        await Task.Delay(100).NoContext();
                    if (promisedState.HasThrownAnException())
                        throw promisedState.Exception;
                    cache = promisedCache.Cache;
                    if (cache == null)
                        return default;
                    if (MemoryIsActive)
                        await InMemory.UpdateAsync(keyString, cache, default);
                    Promised.TryRemove(keyString, out _);
                }
            }
            if (cache != null)
                return cache;
            else
                return MemoryIsActive ? await InMemory.InstanceAsync(keyString).NoContext() : await InCloud.InstanceAsync(keyString).NoContext();

            async Task<bool> ExistsInCloud()
            {
                cloudChecked = true;
                if (CloudIsActive)
                {
                    MultitonStatus<TCache> result = await InCloud.ExistsAsync(keyString).NoContext();
                    if (result.Cache != null)
                        cache = result.Cache;
                    return result.IsOk;
                }
                return false;
            }
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
                return (await InMemory.ExistsAsync(keyString).NoContext()).IsOk;
            else if (CloudIsActive)
                return (await InCloud.ExistsAsync(keyString).NoContext()).IsOk;
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
            public IMultitonIntegration<TCache> InCloud { get; }
            private TCache CachedData;
            public TCache GetCachedData()
                => this.CachedData;
            public Instancer(IMultitonKey<TCache> key, string keyString, bool cloudChecked, IMultitonIntegration<TCache> inCloud)
            {
                this.Key = key;
                this.KeyString = keyString;
                this.InCloud = inCloud;
                this.CloudIsActive = inCloud != null;
                this.CloudChecked = cloudChecked;
            }
            public async Task Execute(object state)
            {
                PromisedState promisedState = (PromisedState)state;
                try
                {
                    bool fromCloud = false;
                    if (await ExistsInCloud())
                    {
                        if (this.CachedData == null)
                            this.CachedData = await InCloud.InstanceAsync(KeyString);
                    }
                    else
                        this.CachedData = await Key.FetchAsync();
                    if (CloudIsActive && !fromCloud)
                        await InCloud.UpdateAsync(KeyString, this.CachedData, default);
                    promisedState.Status = 2;

                    async Task<bool> ExistsInCloud()
                    {
                        if (CloudIsActive && !CloudChecked)
                        {
                            MultitonStatus<TCache> result = await InCloud.ExistsAsync(KeyString);
                            if (result.IsOk)
                            {
                                fromCloud = true;
                                if (result.Cache != null)
                                    this.CachedData = result.Cache;
                                return true;
                            }
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    promisedState.Exception = ex;
                    promisedState.Status = 3;
                }
            }
        }
        private class PromisedState
        {
            public int Status { get; set; }
            public Exception Exception { get; set; }
            public bool IsCompleted() => this.Status == 2 || this.HasThrownAnException();
            public bool HasThrownAnException() => this.Status == 3;
            public bool IsStarted() => this.Status == 1;
        }

        private class PromisedCache
        {
            public Instancer Instance { get; }
            public TCache Cache => Instance.GetCachedData();
            private readonly WaitCallback Executor;
            public PromisedState PromisedState { get; } = new PromisedState();
            public PromisedCache(Instancer instance)
            {
                this.Instance = instance;
                this.Executor = state =>
                {
                    instance.Execute(state).ToResult();
                };
            }
            private static readonly object TrafficLight = new object();
            public PromisedState Run()
            {
                if (this.PromisedState.Status == 0)
                {
                    lock (TrafficLight)
                        if (this.PromisedState.Status == 0)
                        {
                            this.PromisedState.Status = 1;
                            ThreadPool.UnsafeQueueUserWorkItem(this.Executor, this.PromisedState);
                        }
                }
                return this.PromisedState;
            }
        }
    }
}
