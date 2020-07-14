using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    public abstract class WebTelemetry : Telemetry
    {
        internal IHttpContextAccessor HttpContenxtAccessor { get; }
        public WebTelemetry(IHttpContextAccessor httpContextAccessor)
            => this.HttpContenxtAccessor = httpContextAccessor;
    }
    public static class WebTelemetryExtensions
    {
        public static async Task TrackRequestWithBody<TTelemetry>(this TTelemetry telemetry, Installation installation = Installation.Default)
           where TTelemetry : WebTelemetry
        {
            string body = null;
            if (telemetry.HttpContenxtAccessor.HttpContext.Request.ContentLength > 0)
            {
                using MemoryStream memoryStream = new MemoryStream();
                telemetry.HttpContenxtAccessor.HttpContext.Request.Body.CopyTo(memoryStream);
                using StreamReader streamReader = new StreamReader(memoryStream);
                body = await streamReader.ReadToEndAsync();
            }
            telemetry.Track(new RequestTelemetry()
            {
                Headers = telemetry.HttpContenxtAccessor.HttpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                BodyLength = telemetry.HttpContenxtAccessor.HttpContext.Request.ContentLength ?? 0,
                Content = body,
                Method = telemetry.HttpContenxtAccessor.HttpContext.Request.Method,
                RequestUri = telemetry.HttpContenxtAccessor.HttpContext.Request.Path.Value,
                Version = telemetry.HttpContenxtAccessor.HttpContext.Request.Protocol
            }, installation);
        }
        public static void TrackRequest<TTelemetry>(this TTelemetry telemetry, Installation installation = Installation.Default)
           where TTelemetry : WebTelemetry
        {
            telemetry.Track(new RequestTelemetry()
            {
                Headers = telemetry.HttpContenxtAccessor.HttpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                BodyLength = telemetry.HttpContenxtAccessor.HttpContext.Request.Body.Length,
                Method = telemetry.HttpContenxtAccessor.HttpContext.Request.Method,
                RequestUri = telemetry.HttpContenxtAccessor.HttpContext.Request.Path.Value,
                Version = telemetry.HttpContenxtAccessor.HttpContext.Request.Protocol
            }, installation);
        }
    }
}