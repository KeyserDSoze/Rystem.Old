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
    internal static class RepositoryCommand
    {
        public const string Get = "get";
        public const string List = "list";
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
    }
    [ApiController]
    [Route("api/[controller]")]
    public abstract class RepositoryController<TModel> : Controller
    {
        private static readonly ConcurrentDictionary<string, bool> NotImplemented = new ConcurrentDictionary<string, bool>();
        private bool IsImplemented(string method)
            => !NotImplemented.ContainsKey(method);
        private static bool hasCache = true;
        private bool HasCache(string method)
            => hasCache && (method == RepositoryCommand.Get || method == RepositoryCommand.List);
        private bool HasSetCache(string method)
            => hasCache && EntityResponse != null && !EntityResponse.IsNotOk && (method == RepositoryCommand.Create || method == RepositoryCommand.Delete || method == RepositoryCommand.Update);
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
                if (method == RepositoryCommand.Get)
                    model = (T)await GetKey<object>(async () => await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"{RepositoryCommand.Get}-{id}", typeof(TModel).Name).InstanceAsync().NoContext();
                else if (method == RepositoryCommand.List)
                    model = (T)await GetKey<object>(async () => await this.ListAsync().NoContext(), cache.CacheConfiguration, RepositoryCommand.List, typeof(TModel).Name).InstanceAsync().NoContext();
                else
                {
                    List<Task> tasks = new List<Task>();
                    if (method != RepositoryCommand.Delete)
                        tasks.Add(GetKey<object>(async () => await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"{RepositoryCommand.Get}-{id}", typeof(TModel).Name).RestoreAsync());
                    else
                        tasks.Add(GetKey<object>(null, cache.CacheConfiguration, $"get-{id}", typeof(TModel).Name).RemoveAsync());
                    tasks.Add(GetKey<object>(async () => await this.ListAsync().NoContext(), cache.CacheConfiguration, RepositoryCommand.List, typeof(TModel).Name).RestoreAsync());
                    await Task.WhenAll(tasks);
                }
            }
            else
            {
                if (method == RepositoryCommand.Get)
                    model = await GetKey(async () => (T)(object)await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"{RepositoryCommand.Get}-{id}").InstanceAsync().NoContext();
                else if (method == RepositoryCommand.List)
                    model = await GetKey(async () => (T)await this.ListAsync().NoContext(), cache.CacheConfiguration, RepositoryCommand.List).InstanceAsync().NoContext();
                else
                {
                    List<Task> tasks = new List<Task>();
                    if (method != RepositoryCommand.Delete)
                        tasks.Add(GetKey(async () => (T)(object)await this.GetAsync(id).NoContext(), cache.CacheConfiguration, $"{RepositoryCommand.Get}-{id}").RestoreAsync());
                    else
                        tasks.Add(GetKey<T>(null, cache.CacheConfiguration, $"{RepositoryCommand.Get}-{id}").RemoveAsync());
                    tasks.Add(GetKey(async () => ((IEnumerable)await this.ListAsync().NoContext()).Cast<T>(), cache.CacheConfiguration, RepositoryCommand.List).RestoreAsync());
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
        private async Task<IActionResult> ExecuteAsync<T>(string method, Func<Task<T>> action, string id = null)
        {
            if (IsImplemented(method))
            {
                try
                {
                    if (HasCache(method))
                    {
                        var cacheResponse = await this.DealWithCache<T>(method, id).NoContext();
                        if (cacheResponse != null)
                            return Ok(cacheResponse);
                    }
                    var response = await action.Invoke().NoContext();
                    if (response == null && (method == RepositoryCommand.Get || method == RepositoryCommand.List))
                        return NotFound();
                    else
                    {
                        if (HasSetCache(method))
                            await this.DealWithCache<T>(method, EntityResponse.Id).NoContext();
                        if (EntityResponse != null && EntityResponse.IsNotOk)
                            return NotFound();
                        else
                        {
                            if (method == RepositoryCommand.Create)
                                return StatusCode(StatusCodes.Status201Created);
                            else if (method == RepositoryCommand.Update || method == RepositoryCommand.Delete)
                                return Ok();
                            else
                                return Ok(response);
                        }
                    }
                }
                catch (NotImplementedException)
                {
                    NotImplemented.TryAdd(method, true);
                    return StatusCode(StatusCodes.Status501NotImplemented);
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            else
                return StatusCode(StatusCodes.Status501NotImplemented);
        }

        protected abstract Task<TModel> GetAsync(string id);
        protected abstract Task<IEnumerable<TModel>> ListAsync();
        protected abstract Task<EntityResponse> CreateAsync(TModel entity);
        protected abstract Task<EntityResponse> UpdateAsync(TModel entity);
        protected abstract Task<EntityResponse> DeleteAsync(string id);

        private EntityResponse EntityResponse;
        [HttpGet]
        public async Task<IActionResult> Get()
            => await ExecuteAsync(RepositoryCommand.List, async () => await this.ListAsync());
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
            => await ExecuteAsync(RepositoryCommand.Get, async () => await this.GetAsync(id), id);
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TModel model)
        {
            if (model != null)
                return await ExecuteAsync(RepositoryCommand.Create, async () => { EntityResponse = await this.CreateAsync(model); return model; });
            else
                return BadRequest();
        }
        [HttpPatch]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TModel model)
        {
            if (model != null)
                return await ExecuteAsync(RepositoryCommand.Update, async () => { EntityResponse = await this.UpdateAsync(model); return model; });
            else
                return BadRequest();
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id != null)
                return await ExecuteAsync<TModel>(RepositoryCommand.Delete, async () => { EntityResponse = await this.DeleteAsync(id); EntityResponse.Id = id; return default; }, id);
            else
                return BadRequest();
        }
    }
}