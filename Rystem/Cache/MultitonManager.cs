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
        private readonly IMultitonIntegrationAsync<TCache> InCloud;
        private readonly bool CloudIsActive = false;
        private readonly MultitonProperties Configuration;
        public MultitonManager(MultitonProperties configuration)
        {
            this.Configuration = configuration;
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
        private async Task<TCache> InstanceWithoutConsistencyAsync(IMultitonKey<TCache> key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                if (InMemory.Exists(keyString).IsOk)
                    return InMemory.Instance(keyString);
            if (CloudIsActive)
            {
                var responseFromCloud = await InCloud.ExistsAsync(keyString).NoContext();
                if (responseFromCloud.IsOk)
                    return responseFromCloud.Cache != null ? responseFromCloud.Cache : await InCloud.InstanceAsync(keyString).NoContext();
            }
            TCache cache = await key.FetchAsync().NoContext();
            if (MemoryIsActive)
                InMemory.Update(keyString, cache, default);
            if (CloudIsActive)
                await InCloud.UpdateAsync(keyString, cache, default).NoContext();
            return cache;
        }
        public async Task<TCache> InstanceAsync(IMultitonKey<TCache> key)
        {
            if (this.Configuration.Consistency == CacheConsistency.Never)
                return await InstanceWithoutConsistencyAsync(key).NoContext();

            TCache cache = default;
            string keyString = key.ToKeyString();
            bool cloudChecked = false;
            if (!ExistsInMemory() || !await ExistsInCloud().NoContext())
            {
                if (await ExistsInCloud().NoContext())
                    UpdateInMemory();
                else
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
                        if (promisedState.HasEmptyResponse())
                            return cache;
                        cache = promisedCache.Cache;
                        if (cache == null)
                            return cache;
                        UpdateInMemory();
                        Promised.TryRemove(keyString, out _);
                    }
                }
            }
            if (cache != null)
                return cache;
            else
                return MemoryIsActive ? InMemory.Instance(keyString) : await InCloud.InstanceAsync(keyString).NoContext();

            bool ExistsInMemory()
                => MemoryIsActive && InMemory.Exists(keyString).IsOk;
            void UpdateInMemory()
            {
                if (MemoryIsActive)
                    InMemory.Update(keyString, cache, default);
            }
            async Task<bool> ExistsInCloud()
            {
                if (!cloudChecked)
                {
                    cloudChecked = true;
                    if (CloudIsActive)
                    {
                        MultitonStatus<TCache> result = await InCloud.ExistsAsync(keyString).NoContext();
                        if (result.Cache != null)
                            cache = result.Cache;
                        else
                            cache = await InCloud.InstanceAsync(keyString).NoContext();
                        return result.IsOk;
                    }
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
                result |= InMemory.Update(keyString, value, expiringTime);
            if (CloudIsActive)
                result |= await InCloud.UpdateAsync(keyString, value, expiringTime).NoContext();
            return result;
        }
        public async Task<bool> ExistsAsync(IMultiKey key)
        {
            string keyString = key.ToKeyString();
            if (MemoryIsActive)
                return InMemory.Exists(keyString).IsOk;
            else if (CloudIsActive)
                return (await InCloud.ExistsAsync(keyString).NoContext()).IsOk;
            return false;
        }
        public async Task<bool> DeleteAsync(IMultiKey key)
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
            public IMultitonKey<TCache> Key { get; }
            public string KeyString { get; }
            public bool CloudIsActive { get; }
            public bool CloudChecked { get; }
            public IMultitonIntegrationAsync<TCache> InCloud { get; }
            private TCache CachedData;
            public TCache GetCachedData()
                => this.CachedData;
            public Instancer(IMultitonKey<TCache> key, string keyString, bool cloudChecked, IMultitonIntegrationAsync<TCache> inCloud)
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
            //private readonly Thread Thread;
            public PromisedCache(Instancer instance)
            {
                this.Instance = instance;
                this.Executor = state => instance.Execute(state).ToResult();
                //this.Thread = new Thread(() => instance.Execute(this.PromisedState).ToResult());
                //this.Thread.Priority = ThreadPriority.Highest;
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
                            //this.Thread.Start();
                        }
                }
                return this.PromisedState;
            }
        }
    }
}
