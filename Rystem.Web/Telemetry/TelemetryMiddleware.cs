using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    public class TelemetryMiddleware : IMiddleware
    {
        private readonly Telemetry Telemetry;
        private readonly IHttpContextAccessor HttpContextAccessor;
        public TelemetryMiddleware(Telemetry telemetry, IHttpContextAccessor httpContextAccessor)
        {
            this.Telemetry = telemetry;
            this.HttpContextAccessor = httpContextAccessor;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (next != null)
                await next.Invoke(context);
            try
            {
                await this.Telemetry.TrackRequest(HttpContextAccessor.HttpContext.Request);
                await this.Telemetry.StopAsync();
            }
            catch (Exception ex)
            {
                string olaf = ex.ToString();
            }
        }
    }
}
