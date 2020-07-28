using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        private async Task<T> ExecuteAsync<T>(string method, Func<Task<T>> action)
        {
            if (IsImplemented(method))
            {
                try
                {
                    var response = await action.Invoke().NoContext();
                    if (response == null)
                        Response.StatusCode = StatusCodes.Status404NotFound;
                    else
                        return response;
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
        protected abstract Task CreateAsync(TModel entity);
        protected abstract Task DeleteAsync(TModel entity);
        protected abstract Task UpdateAsync(TModel entity);
        [HttpGet]
        public async Task<IEnumerable<TModel>> Get()
            => await ExecuteAsync("list", async () => await this.ListAsync());
        [HttpGet]
        [Route("{id}")]
        public async Task<TModel> Get(string id)
            => await ExecuteAsync("get", async () => await this.GetAsync(id));
        [HttpPost]
        public async Task Post([FromBody] TModel model)
           => await ExecuteAsync("create", async () => { await this.CreateAsync(model); return true; });
        [HttpPatch]
        [HttpPut]
        public async Task Update([FromBody] TModel model)
            => await ExecuteAsync("update", async () => { await this.UpdateAsync(model); return true; });
        [HttpDelete]
        public async Task Delete([FromBody] TModel model)
            => await ExecuteAsync("delete", async () => { await this.DeleteAsync(model); return true; });
    }
}