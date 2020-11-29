using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Rystem;
using System.Net.Http.Headers;

namespace System
{
    public static partial class RepositoryPatternCallerExtensions
    {
        private static async Task<TEntity> CallAsync<TEntity>(IRepositoryCaller repositoryCaller, Func<Task<TEntity>> action)
        {
            try
            {
                return await action.Invoke().NoContext();
            }
            catch (WebException webException)
            {
                repositoryCaller.ErrorResponse = (await webException.Response.GetResponseStream().GetValueAsync().NoContext()).FromJson<RepositoryPatternErrorResponse>();
                return default;
            }
        }
        public static Task<TModel> GetAsync<TModel>(this IRepositoryCaller repositoryCaller, string id, int timeout = 30000, IDictionary<string, string> headers = null)
            => CallAsync(repositoryCaller,
                () => new Uri($"{repositoryCaller.Uri}/{id}").InvokeHttpAsync<TModel>(timeout, HttpMethod.Get, headers));
        public static Task<IEnumerable<TModel>> ListAsync<TModel>(this IRepositoryCaller repositoryCaller, int timeout = 30000, IDictionary<string, string> headers = null)
            => CallAsync(repositoryCaller,
                () => repositoryCaller.Uri.InvokeHttpAsync<IEnumerable<TModel>>(timeout, HttpMethod.Get, headers));
        public static Task CreateAsync<TModel>(this IRepositoryCaller repositoryCaller, TModel entity, int timeout = 30000, IDictionary<string, string> headers = null)
            => CallAsync(repositoryCaller,
                () => repositoryCaller.Uri.InvokeHttpAsync(timeout, HttpMethod.Post, headers, "application/json", entity.ToJson()));
        public static Task UpdateAsync<TModel>(this IRepositoryCaller repositoryCaller, TModel entity, int timeout = 30000, IDictionary<string, string> headers = null)
            => CallAsync(repositoryCaller,
                () => repositoryCaller.Uri.InvokeHttpAsync(timeout, HttpMethod.Patch, headers, "application/json", entity.ToJson()));
        public static Task DeleteAsync(this IRepositoryCaller repositoryCaller, string id, int timeout = 30000, IDictionary<string, string> headers = null)
            => CallAsync(repositoryCaller,
                () => new Uri($"{repositoryCaller.Uri}/{id}").InvokeHttpAsync(timeout, HttpMethod.Delete, headers));
    }
    public static partial class RepositoryPatternCallerExtensions
    {
        public static TModel Get<TModel>(this IRepositoryCaller repositoryCaller, string id, int timeout = 30000, IDictionary<string, string> headers = null)
            => repositoryCaller.GetAsync<TModel>(id, timeout, headers).ToResult();
        public static IEnumerable<TModel> List<TModel>(this IRepositoryCaller repositoryCaller, int timeout = 30000, IDictionary<string, string> headers = null)
            => repositoryCaller.ListAsync<TModel>(timeout, headers).ToResult();
        public static void Create<TModel>(this IRepositoryCaller repositoryCaller, TModel entity, int timeout = 30000, IDictionary<string, string> headers = null)
            => repositoryCaller.CreateAsync(entity, timeout, headers).ToResult();
        public static void Update<TModel>(this IRepositoryCaller repositoryCaller, TModel entity, int timeout = 30000, IDictionary<string, string> headers = null)
            => repositoryCaller.UpdateAsync(entity, timeout, headers).ToResult();
        public static void Delete(this IRepositoryCaller repositoryCaller, string id, int timeout = 30000, IDictionary<string, string> headers = null)
            => repositoryCaller.DeleteAsync(id, timeout, headers).ToResult();
    }
}