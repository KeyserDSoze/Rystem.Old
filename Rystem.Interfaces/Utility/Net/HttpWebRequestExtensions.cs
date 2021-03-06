using Rystem;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    public static partial class HttpWebRequestExtensions
    {
        public static async Task<string> InvokeHttpAsync(this Uri uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, string body = null, bool keepAlive = false)
            => await uri.InvokeHttpAsyncWithStream(timeout, method, headers, contentType, body != null ? new MemoryStream(Encoding.UTF8.GetBytes(body)) : null, keepAlive);
        public static async Task<string> InvokeHttpAsyncWithStream(this Uri uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, Stream body = null, bool keepAlive = false)
        {
            using (HttpWebResponse httpWebResponse = await uri.PrepareRequest(timeout, method, headers, contentType, body, keepAlive))
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                return await streamReader.ReadToEndAsync().NoContext();
        }
        private static async Task<HttpWebResponse> PrepareRequest(this Uri uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, Stream body = null, bool keepAlive = false)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Timeout = timeout;
            httpWebRequest.KeepAlive = keepAlive;
            if (method != null)
                httpWebRequest.Method = method.ToString();
            if (contentType != null)
                httpWebRequest.ContentType = contentType;
            if (headers != null)
                foreach (var header in headers)
                {
                    if (header.Key.ToLower() == "host")
                        continue;
                    if (header.Key.ToLower() == "referer")
                        httpWebRequest.Headers.Add(header.Key, $"{uri.Scheme}://{uri.Host}");
                    else
                        httpWebRequest.Headers.Add(header.Key, string.Join(",", header.Value));
                }
            if (body != null)
            {
                using (Stream requestStream = await httpWebRequest.GetRequestStreamAsync())
                    await body.CopyToAsync(requestStream);
            }
            return (HttpWebResponse)(await httpWebRequest.GetResponseAsync().NoContext());
        }
        public static async Task<(HttpWebResponse Response, byte[] Body)> InvokeHttpToByteAsync(this Uri uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, Stream body = null, bool keepAlive = false)
        {
            HttpWebResponse httpWebResponse = await uri.PrepareRequest(timeout, method, headers, contentType, body, keepAlive);
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }
                return (httpWebResponse, bytes);
            }
        }
        public static async Task<string> InvokeHttpPostAsync<TEntity>(this TEntity entity, Uri uri, int timeout = 30_000, IDictionary<string, string> headers = null, bool keepAlive = false)
            => await uri.InvokeHttpAsync(timeout, HttpMethod.Post, headers, "application/json; charset=UTF-8", entity.ToDefaultJson(), keepAlive).NoContext();

        public static async Task<TEntity> InvokeHttpAsync<TEntity>(this Uri uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, string body = null, bool keepAlive = false)
           => (await uri.InvokeHttpAsync(timeout, method, headers, contentType, body, keepAlive).NoContext()).FromDefaultJson<TEntity>();
        public static async Task<TResponse> InvokeHttpPostAsync<TEntity, TResponse>(this TEntity entity, Uri uri, int timeout = 30_000, IDictionary<string, string> headers = null, bool keepAlive = false)
            => (await entity.InvokeHttpPostAsync(uri, timeout, headers, keepAlive).NoContext()).FromDefaultJson<TResponse>();
    }
    public static partial class HttpWebRequestExtensions
    {
        public static async Task<string> InvokeHttpAsync(this string uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, string body = null, bool keepAlive = false)
            => await new Uri(uri).InvokeHttpAsync(timeout, method, headers, contentType, body, keepAlive).NoContext();
        public static async Task<string> InvokeHttpPostAsync<TEntity>(this TEntity entity, string uri, int timeout = 30_000, IDictionary<string, string> headers = null, bool keepAlive = false)
            => await entity.InvokeHttpPostAsync(new Uri(uri), timeout, headers, keepAlive).NoContext();

        public static async Task<TEntity> InvokeHttpAsync<TEntity>(this string uri, int timeout = 30_000, HttpMethod method = null, IDictionary<string, string> headers = null, string contentType = null, string body = null, bool keepAlive = false)
          => (await uri.InvokeHttpAsync(timeout, method, headers, contentType, body, keepAlive).NoContext()).FromDefaultJson<TEntity>();
        public static async Task<TResponse> InvokeHttpPostAsync<TEntity, TResponse>(this TEntity entity, string uri, int timeout = 30_000, IDictionary<string, string> headers = null, bool keepAlive = false)
            => (await entity.InvokeHttpPostAsync(uri, timeout, headers, keepAlive).NoContext()).FromDefaultJson<TResponse>();
    }
}