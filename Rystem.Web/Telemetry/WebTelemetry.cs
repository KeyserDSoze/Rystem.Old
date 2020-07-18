using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    public static class WebTelemetryExtensions
    {
        public static async Task TrackRequestWithBody<TTelemetry>(this TTelemetry telemetry, HttpRequest httpRequest, Installation installation = Installation.Default)
           where TTelemetry : Telemetry
        {
            string body = null;
            if (httpRequest.ContentLength > 0)
            {
                using MemoryStream memoryStream = new MemoryStream();
                httpRequest.Body.CopyTo(memoryStream);
                using StreamReader streamReader = new StreamReader(memoryStream);
                body = await streamReader.ReadToEndAsync();
            }
            telemetry.Track(new RequestTelemetry()
            {
                Headers = httpRequest.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                BodyLength = httpRequest.ContentLength ?? 0,
                Content = body,
                Method = httpRequest.Method,
                RequestUri = httpRequest.Path.Value,
                Version = httpRequest.Protocol
            }, installation);
        }
        public static void TrackRequest<TTelemetry>(this TTelemetry telemetry, HttpRequest httpRequest, Installation installation = Installation.Default)
           where TTelemetry : Telemetry
        {
            telemetry.Track(new RequestTelemetry()
            {
                Headers = httpRequest.Headers?.ToDictionary(x => x.Key, x => x.Value.ToString()),
                BodyLength = httpRequest.Body.Length,
                Method = httpRequest.Method,
                RequestUri = httpRequest.Path.Value,
                Version = httpRequest.Protocol
            }, installation);
        }
    }
}