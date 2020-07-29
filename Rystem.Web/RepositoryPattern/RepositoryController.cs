using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rystem.Cache;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class RepositoryController<TModel> : Controller
    {
        private static readonly ConcurrentDictionary<string, bool> NotImplemented = new ConcurrentDictionary<string, bool>();
        private bool IsImplemented(string method)
            => !NotImplemented.ContainsKey(method);
        private static bool hasCache = true;
        private bool HasCache(string method) => hasCache && (method == "get" || method == "list");
        private bool HasSetCache(string method) => hasCache && EntityResponse != null && (method == "create" || method == "delete" || method == "update");
        private async Task<T> DealWithCache<T>(string method, string id)
        {
            var cache = RepositoryPatternExtensions.Options.GetCache<T>();
            if (cache == null)
            {
                hasCache = false;
                return default;
            }
            T model = default;
            if (cache.IsDefault)
            {
                if (method == "get")
                    model = (T)await GetKey<object>(async () => await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"get-{id}", typeof(TModel).Name).InstanceAsync().NoContext();
                else if (method == "list")
                    model = (T)await GetKey<object>(async () => await this.ListAsync().NoContext(), cache.CacheConfiguration, $"list", typeof(TModel).Name).InstanceAsync().NoContext();
                else
                {
                    List<Task> tasks = new List<Task>();
                    if (method != "delete")
                        tasks.Add(GetKey<object>(async () => await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"get-{id}", typeof(TModel).Name).RestoreAsync());
                    else
                        tasks.Add(GetKey<object>(null, cache.CacheConfiguration, $"get-{id}", typeof(TModel).Name).RemoveAsync());
                    tasks.Add(GetKey<object>(async () => await this.ListAsync().NoContext(), cache.CacheConfiguration, $"list", typeof(TModel).Name).RestoreAsync());
                    await Task.WhenAll(tasks);
                }
            }
            else
            {
                if (method == "get")
                    model = await GetKey(async () => (T)(object)await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"get-{id}").InstanceAsync().NoContext();
                else if (method == "list")
                    model = await GetKey(async () => (T)await this.ListAsync().NoContext(), cache.CacheConfiguration, $"list").InstanceAsync().NoContext();
                else
                {
                    List<Task> tasks = new List<Task>();
                    if (method != "delete")
                        tasks.Add(GetKey(async () => (T)(object)await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"get-{id}").RestoreAsync());
                    else
                        tasks.Add(GetKey<T>(null, cache.CacheConfiguration, $"get-{id}").RemoveAsync());
                    tasks.Add(GetKey(async () => ((IEnumerable)await this.ListAsync().NoContext()).Cast<T>(), cache.CacheConfiguration, $"list").RestoreAsync());
                    await Task.WhenAll(tasks);
                }
            }
            return model;
        }
        private static ICacheKey<TEntity> GetKey<TEntity>(Func<Task<TEntity>> fetcher, ConfigurationBuilder configurationBuilder, string id, string keyName = null)
        {
            return new RepositoryCacheKey<TEntity>()
            {
                Key = $"{keyName ?? typeof(TEntity).Name}-{id}",
                Fetcher = fetcher,
                ConfigurationBuilder = configurationBuilder
            };
        }
        private async Task<T> ExecuteAsync<T>(string method, Func<Task<T>> action, string id = null)
        {
            if (IsImplemented(method))
            {
                try
                {
                    if (HasCache(method))
                    {
                        var cacheResponse = await this.DealWithCache<T>(method, id).NoContext();
                        if (cacheResponse != null)
                            return cacheResponse;
                    }
                    var response = await action.Invoke().NoContext();
                    if (response == null)
                        Response.StatusCode = StatusCodes.Status404NotFound;
                    else
                    {
                        if (HasSetCache(method))
                            await this.DealWithCache<T>(method, EntityResponse.Id).NoContext();
                        return response;
                    }
                }
                catch (NotImplementedException)
                {
                    NotImplemented.TryAdd(method, true);
                    Response.StatusCode = StatusCodes.Status501NotImplemented;
                }
                catch (Exception)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
                Response.StatusCode = StatusCodes.Status501NotImplemented;
            return default;
        }
        protected abstract Task<TModel> GetAsync(string id);
        protected abstract Task<IEnumerable<TModel>> ListAsync();
        protected abstract Task<EntityResponse> CreateAsync(TModel entity);
        protected abstract Task<EntityResponse> DeleteAsync(TModel entity);
        protected abstract Task<EntityResponse> UpdateAsync(TModel entity);
        private EntityResponse EntityResponse;
        [HttpGet]
        public async Task<IEnumerable<TModel>> Get()
            => await ExecuteAsync("list", async () => await this.ListAsync());
        [HttpGet]
        [Route("{id}")]
        public async Task<TModel> Get(string id)
            => await ExecuteAsync("get", async () => await this.GetAsync(id), id);
        [HttpPost]
        public async Task Post([FromBody] TModel model)
           => await ExecuteAsync("create", async () => { EntityResponse = await this.CreateAsync(model); return model; });
        [HttpPatch]
        [HttpPut]
        public async Task Update([FromBody] TModel model)
            => await ExecuteAsync("update", async () => { EntityResponse = await this.UpdateAsync(model); return model; });
        [HttpDelete]
        public async Task Delete([FromBody] TModel model)
            => await ExecuteAsync("delete", async () => { EntityResponse = await this.DeleteAsync(model); return model; });
    }
}