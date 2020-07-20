using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        public static async Task TrackRequest<TTelemetry>(this TTelemetry telemetry, HttpRequest httpRequest, Installation installation = Installation.Default)
           where TTelemetry : Telemetry
        {
            MemoryStream memoryStream = new MemoryStream();
            await httpRequest.Body.CopyToAsync(memoryStream);
            telemetry.Track(new RequestTelemetry()
            {
                Headers = httpRequest.Headers?.ToDictionary(x => x.Key, x => x.Value.ToString()),
                BodyLength = memoryStream.Length,
                Content = await memoryStream.GetValueAsync(),
                Method = httpRequest.Method,
                RequestUri = httpRequest.Path.Value,
                Version = httpRequest.Protocol
            }, installation);
        }
        public static IServiceCollection AddRystemTelemetry<TTelemetry>(this IServiceCollection services)
            where TTelemetry : Telemetry, new()
        {
            services.AddHttpContextAccessor();
            services.AddScoped<Telemetry, TTelemetry>(x => Telemetry.CreateNew<TTelemetry>());
            services.AddScoped<TelemetryMiddleware>();
            return services;
        }
        public static void UseRystemTelemetry(this IApplicationBuilder app)
        {
            app.UseMiddleware<TelemetryMiddleware>();
        }
    }
}