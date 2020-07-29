using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rystem.Cache;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web
{
    public static class RepositoryPatternExtensions
    {
        internal static RepositoryPatternOption Options = new RepositoryPatternOption();
        public static IServiceCollection AddRepositoryPattern(this IServiceCollection services, Action<RepositoryPatternOption> options = null, Action<SwaggerGenOptions> swaggerGenOptions = null)
        {
            services.AddSwaggerGen(swaggerGenOptions);
            options?.Invoke(Options);
            return services;
        }
        public static IApplicationBuilder UseRepositoryPattern(this IApplicationBuilder app, Action<SwaggerOptions> options = null, Action<SwaggerUIOptions> swaggerUIOptions = null)
        {
            app.UseSwagger(options);
            app.UseSwaggerUI(swaggerUIOptions ?? (c =>
             {
                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api V1");
             }));
            return app;
        }
    }
}