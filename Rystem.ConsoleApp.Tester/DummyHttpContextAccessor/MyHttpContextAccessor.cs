using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.DummyHttpContextAccessor
{
    class MyHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = new MyHttpContext(new HeaderDictionary());
    }
    public sealed class MyQueryCollection : IQueryCollection
    {
        private Dictionary<string, string> Dictionaries = new Dictionary<string, string>();
        public StringValues this[string key] => Dictionaries[key];

        public int Count => Dictionaries.Count;
        public MyQueryCollection(string path)
        {
            if (path != null)
                foreach (string query in path.Split('?').Last().Split('&').Where(x => !string.IsNullOrWhiteSpace(x)))
                    if (!Dictionaries.ContainsKey(query.Split('=').First()))
                        Dictionaries.Add(query.Split('=').First(), WebUtility.UrlDecode(query.Split('=').Last()));
        }
        public ICollection<string> Keys => Dictionaries.Keys;

        public bool ContainsKey(string key)
        => Dictionaries.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class HeadersDictionary : IHeaderDictionary
    {
        private Dictionary<string, StringValues> Headers = new Dictionary<string, StringValues>();
        public StringValues this[string key] { get => Headers[key]; set => Headers[key] = value; }

        public long? ContentLength { get => Headers.Count; set => throw new NotImplementedException(); }

        public ICollection<string> Keys => Headers.Keys;

        public ICollection<StringValues> Values => Headers.Values;

        public int Count => Headers.Count;

        public bool IsReadOnly => true;

        public void Add(string key, StringValues value)
            => Headers.Add(key, value);

        public void Add(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return Headers.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            return Headers.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Headers.Remove(key);
        }

        public bool Remove(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            return !string.IsNullOrEmpty(value = Headers[key]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Headers.GetEnumerator();
        }
    }
    public sealed class MyCookieCollection : IRequestCookieCollection, IResponseCookies
    {
        private Dictionary<string, string> Cookies = new Dictionary<string, string>();
        public string this[string key]
            => this.Cookies.ContainsKey(key) ? this.Cookies[key] : null;

        public int Count => this.Cookies.Count;

        public ICollection<string> Keys => this.Cookies.Keys;

        public void Append(string key, string value)
        {
            this.Cookies.Add(key, value);
        }

        public void Append(string key, string value, CookieOptions options)
        {
            this.Cookies.Add(key, value);
        }

        public bool ContainsKey(string key)
            => this.Cookies.ContainsKey(key);

        public void Delete(string key)
        {
            this.Cookies.Remove(key);
        }

        public void Delete(string key, CookieOptions options)
        {
            this.Cookies.Remove(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => this.Cookies.GetEnumerator();

        public bool TryGetValue(string key, out string value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    public sealed class MyHttpRequest : HttpRequest
    {
        public override HttpContext HttpContext { get; }
        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; }
        public override IRequestCookieCollection Cookies { get; set; } = new MyCookieCollection();
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; } = new MemoryStream();
        public override bool HasFormContentType => throw new NotImplementedException();
        public override IFormCollection Form { get; set; }
        public MyHttpRequest(HttpContext context, IHeaderDictionary headers)
        {
            this.HttpContext = context;
            this.Headers = headers;
        }
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class MyConnectionInfo : ConnectionInfo
    {
        public override string Id { get; set; } = Rystem.Utility.Alea.GetTimedKey();
        public override IPAddress RemoteIpAddress { get; set; }
        public override int RemotePort { get; set; }
        public override IPAddress LocalIpAddress { get; set; }
        public override int LocalPort { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override X509Certificate2 ClientCertificate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class MyHttpResponse : HttpResponse
    {
        public override HttpContext HttpContext { get; }
        public MyHttpResponse(HttpContext context)
        {
            this.HttpContext = context;
        }
        public override int StatusCode { get; set; }

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }

        public override IResponseCookies Cookies { get; } = new MyCookieCollection();
        private bool hasStarted = false;
        public override bool HasStarted => this.hasStarted;

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            Console.WriteLine($"Completed State: {state}");
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            Console.WriteLine($"Starting response State: {state}");
            hasStarted = true;
        }

        public override void Redirect(string location, bool permanent)
        {
            Console.WriteLine($"Redirect on: {location} --> permanent: {permanent}");
        }
    }
    public sealed class MyHttpContext : HttpContext
    {
        public override IFeatureCollection Features => throw new NotImplementedException();
        private HttpRequest request;
        public override HttpRequest Request => request ?? (request = new MyHttpRequest(this, this.Headers));
        private IHeaderDictionary Headers;
        public MyHttpContext(IHeaderDictionary headers)
        {
            this.Headers = headers;
        }
        private HttpResponse response;
        public override HttpResponse Response => response ?? (response = new MyHttpResponse(this));
        private ConnectionInfo connectionInfo;
        public override ConnectionInfo Connection => connectionInfo ?? (connectionInfo = new MyConnectionInfo());

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
        public static HttpRequest SimulateRequest(byte[] postBytes, string contentType, IPAddress remoteAddress, int remotePort, IPAddress localAddress, string path = null, HeaderDictionary headers = null, string method = "POST")
        {
            MyHttpContext myHttpContext = new MyHttpContext(headers ?? new HeaderDictionary());
            myHttpContext.Connection.RemoteIpAddress = remoteAddress;
            myHttpContext.Connection.RemotePort = remotePort;
            myHttpContext.Connection.LocalIpAddress = localAddress;
            myHttpContext.Request.Query = new MyQueryCollection(path);
            myHttpContext.Request.Body.Write(postBytes, 0, postBytes.Length);
            myHttpContext.Request.ContentType = contentType;
            myHttpContext.Request.ContentLength = postBytes.Length;
            myHttpContext.Request.Method = method;
            myHttpContext.Request.Body.Flush();
            myHttpContext.Request.Body.Position = 0;
            myHttpContext.Request.QueryString = new QueryString("?" + path?.Split('?').Last());
            return myHttpContext.Request;
        }
    }
}
