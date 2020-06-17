using Rystem;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    public static partial class HttpWebRequestExtensions
    {
        public static async Task<string> InvokeHttpAsync(this Uri uri, int timeout = 30_000, HttpMethod method = null, HttpHeaders headers = null, string contentType = null, string body = null)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Timeout = timeout;
            if (method != null)
                httpWebRequest.Method = method.ToString();
            if (contentType != null)
                httpWebRequest.ContentType = contentType;
            if (headers != null)
                foreach (var header in headers)
                    httpWebRequest.Headers.Add(header.Key, string.Join(",", header.Value));
            if (body != null)
            {
                byte[] postBytes = Encoding.UTF8.GetBytes(body);
                httpWebRequest.ContentLength = postBytes.Length;
                using (Stream requestStream = await httpWebRequest.GetRequestStreamAsync())
                    await requestStream.WriteAsync(postBytes, 0, postBytes.Length).NoContext();
            }
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)(await httpWebRequest.GetResponseAsync().NoContext()))
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                return await streamReader.ReadToEndAsync().NoContext();
        }
        public static async Task<string> InvokeHttpPostAsync<TEntity>(this TEntity entity, Uri uri, int timeout = 30_000, HttpHeaders headers = null)
            => await uri.InvokeHttpAsync(timeout, HttpMethod.Post, headers, "application/json; charset=UTF-8", entity.ToDefaultJson()).NoContext();

        public static async Task<TEntity> InvokeHttpAsync<TEntity>(this Uri uri, int timeout = 30_000, HttpMethod method = null, HttpHeaders headers = null, string contentType = null, string body = null)
           => (await uri.InvokeHttpAsync(timeout, method, headers, contentType, body).NoContext()).FromDefaultJson<TEntity>();
        public static async Task<TResponse> InvokeHttpPostAsync<TEntity, TResponse>(this TEntity entity, Uri uri, int timeout = 30_000, HttpHeaders headers = null)
            => (await entity.InvokeHttpPostAsync(uri, timeout, headers).NoContext()).FromDefaultJson<TResponse>();
    }
    public static partial class HttpWebRequestExtensions
    {
        public static async Task<string> InvokeHttpAsync(this string uri, int timeout = 30_000, HttpMethod method = null, HttpHeaders headers = null, string contentType = null, string body = null)
            => await new Uri(uri).InvokeHttpAsync(timeout, method, headers, contentType, body).NoContext();
        public static async Task<string> InvokeHttpPostAsync<TEntity>(this TEntity entity, string uri, int timeout = 30_000, HttpHeaders headers = null)
            => await entity.InvokeHttpPostAsync(new Uri(uri), timeout, headers).NoContext();

        public static async Task<TEntity> InvokeHttpAsync<TEntity>(this string uri, int timeout = 30_000, HttpMethod method = null, HttpHeaders headers = null, string contentType = null, string body = null)
          => (await uri.InvokeHttpAsync(timeout, method, headers, contentType, body).NoContext()).FromDefaultJson<TEntity>();
        public static async Task<TResponse> InvokeHttpPostAsync<TEntity, TResponse>(this TEntity entity, string uri, int timeout = 30_000, HttpHeaders headers = null)
            => (await entity.InvokeHttpPostAsync(uri, timeout, headers).NoContext()).FromDefaultJson<TResponse>();
    }
}