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
    internal class CacheManager<TCacheKey, TCache> : ICacheManager<TCacheKey, TCache>
        where TCacheKey : ICacheKey<TCache>
    {
        private readonly IMultitonIntegration<TCache> InMemory;
        private bool MemoryIsActive
            => this.Configuration.HasMemory;
        private readonly ICacheIntegrationAsync<TCache> InCloud;
        private bool CloudIsActive
            => this.Configuration.HasCloud;

        public InstallerType InstallerType => InstallerType.Cache;

        private readonly RystemCacheConfiguration Configuration;
        public CacheManager(ConfigurationBuilder builder)
        {
            this.Configuration = builder.GetConfigurations(this.InstallerType)[Installation.Default] as RystemCacheConfiguration;
            if (this.Configuration.HasCloud && this.Configuration.CloudProperties.Namespace == null)
                this.Configuration.CloudProperties.Namespace = typeof(TCache).FullName;
            if (this.Configuration.HasMemory && this.Configuration.MemoryProperties.Namespace == null)
                this.Configuration.MemoryProperties.Namespace = typeof(TCache).FullName;
            if (this.MemoryIsActive)
                InMemory = new InMemory<TCache>(this.Configuration);
            if (this.CloudIsActive)
            {
                if (string.IsNullOrWhiteSpace(Configuration.ConnectionString))
                    throw new ArgumentException($"Value {typeof(TCache).FullName} installed for cloud without a connection string.");
                switch (Configuration.CloudProperties.Type)
                {
                    case CloudCacheType.RedisCache:
                        InCloud = new InRedisCache<TCache>(Configuration);
                        break;
                    case CloudCacheType.TableStorage:
                        InCloud = new InTableStorage<TCache>(Configuration);
                        break;
                    case CloudCacheType.BlobStorage:
                        InCloud = new InBlobStorage<TCache>(Configuration);
                        break;
                    default:
                        throw new NotImplementedException($"CloudType not found {Configuration.CloudProperties.Type}");
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
        private async Task<TCache> InstanceWithoutConsistencyAsync(TCacheKey key, string keyString)
        {
            TCache cache = await key.FetchAsync().NoContext();
            if (MemoryIsActive)
                InMemory.Update(keyString, cache, default);
            if (CloudIsActive)
                await InCloud.UpdateAsync(keyString, cache, default).NoContext();
            return cache;
        }
        private async Task<TCache> InstanceWithConsistencyAsync(TCacheKey key, string keyString)
        {
            try
            {
                TCache cache = default;
                Promised.TryAdd(keyString,
                    new MyLazy<PromisedCache>(() => new PromisedCache(new Instancer(key, keyString, InCloud))));
                Promised.TryGetValue(keyString, out MyLazy<PromisedCache> lazy);
                if (lazy != null)
                {
                    PromisedCache promisedCache = lazy.Value;
                    PromisedState promisedState = default;
                    while (!(promisedState = promisedCache.Run()).IsCompleted())
                        await Task.Delay(100).NoContext();
                    if (promisedState.HasThrownAnException())
                        throw promisedState.Exception;
                    if (promisedState.HasEmptyResponse())
                        return cache;
                    cache = promisedCache.Cache;
                    if (cache == null)
                        return cache;
                    if (MemoryIsActive)
                        InMemory.Update(keyString, cache, default);
                }
                if (cache != null)
                    return cache;
                else if (MemoryIsActive)
                    return InMemory.Instance(keyString);
                else
                    return await InCloud.InstanceAsync(keyString).NoContext();
            }
            finally
            {
                Promised.TryRemove(keyString, out _);
            }
        }
        public async Task<TCache> InstanceAsync(TCacheKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                if (InMemory.Exists(keyString).IsOk)
                    return InMemory.Instance(keyString);
            if (CloudIsActive)
            {
                MultitonStatus<TCache> responseFromCloud = await InCloud.ExistsAsync(keyString).NoContext();
                if (responseFromCloud.IsOk)
                {
                    TCache cache = responseFromCloud.Cache != null ? responseFromCloud.Cache : await InCloud.InstanceAsync(keyString).NoContext();
                    if (MemoryIsActive)
                        InMemory.Update(keyString, cache, default);
                    return cache;
                }
            }
            if (this.Configuration.Consistency == CacheConsistency.Never)
                return await InstanceWithoutConsistencyAsync(key, keyString).NoContext();
            else
                return await InstanceWithConsistencyAsync(key, keyString).NoContext();
        }
        public async Task<bool> UpdateAsync(TCacheKey key, TCache value, TimeSpan expiringTime)
        {
            string keyString = key.ToKeyString();
            if (value == null)
                value = await key.FetchAsync().NoContext();
            bool result = false;
            if (MemoryIsActive)
                result |= InMemory.Update(keyString, value, expiringTime);
            if (CloudIsActive)
                result |= await InCloud.UpdateAsync(keyString, value, expiringTime).NoContext();
            return result;
        }
        public async Task<bool> ExistsAsync(TCacheKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                return InMemory.Exists(keyString).IsOk;
            else if (CloudIsActive)
                return (await InCloud.ExistsAsync(keyString).NoContext()).IsOk;
            return false;
        }
        public async Task<bool> DeleteAsync(TCacheKey key)
        {
            string keyString = key.ToKeyString();
            bool result = false;
            if (MemoryIsActive)
                result |= InMemory.Delete(keyString);
            if (CloudIsActive)
                result |= await InCloud.DeleteAsync(keyString).NoContext();
            return result;
        }
        public async Task<IEnumerable<string>> ListAsync()
        {
            if (CloudIsActive)
                return await InCloud.ListAsync().NoContext();
            if (MemoryIsActive)
                return InMemory.List();
            return null;
        }
        public async Task WarmUp()
            => await InCloud.WarmUp().NoContext();
        private class Instancer
        {
            public ICacheKey<TCache> Key { get; }
            public string KeyString { get; }
            public bool CloudIsActive { get; }
            public ICacheIntegrationAsync<TCache> InCloud { get; }
            private TCache CachedData;
            public TCache GetCachedData()
                => this.CachedData;
            public Instancer(ICacheKey<TCache> key, string keyString, ICacheIntegrationAsync<TCache> inCloud)
            {
                this.Key = key;
                this.KeyString = keyString;
                this.InCloud = inCloud;
                this.CloudIsActive = inCloud != null;
            }
            public async Task Execute(object state)
            {
                PromisedState promisedState = (PromisedState)state;
                try
                {
                    this.CachedData = await Key.FetchAsync().NoContext();
                    if (this.CachedData != null)
                    {
                        if (CloudIsActive)
                            await InCloud.UpdateAsync(KeyString, this.CachedData, default).NoContext();
                        promisedState.Status = PromisedStatus.Executed;
                    }
                    else
                        promisedState.Status = PromisedStatus.Empty;
                }
                catch (Exception ex)
                {
                    promisedState.Exception = ex;
                    promisedState.Status = PromisedStatus.InException;
                }
            }
        }
        private enum PromisedStatus
        {
            Ready,
            InExecution,
            Executed,
            Empty,
            InException
        }
        private class PromisedState
        {
            public PromisedStatus Status { get; set; }
            public Exception Exception { get; set; }
            public bool IsCompleted() => this.Status == PromisedStatus.Executed || this.HasEmptyResponse() || this.HasThrownAnException();
            public bool HasThrownAnException() => this.Status == PromisedStatus.InException;
            public bool HasEmptyResponse() => this.Status == PromisedStatus.Empty;
            public bool IsStarted() => this.Status == PromisedStatus.InExecution;
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
                this.Executor = state => instance.Execute(state).ToResult();
            }
            private static readonly object TrafficLight = new object();
            public PromisedState Run()
            {
                if (this.PromisedState.Status == PromisedStatus.Ready)
                {
                    lock (TrafficLight)
                        if (this.PromisedState.Status == PromisedStatus.Ready)
                        {
                            this.PromisedState.Status = PromisedStatus.InExecution;
                            ThreadPool.UnsafeQueueUserWorkItem(this.Executor, this.PromisedState);
                        }
                }
                return this.PromisedState;
            }
        }
    }
}
