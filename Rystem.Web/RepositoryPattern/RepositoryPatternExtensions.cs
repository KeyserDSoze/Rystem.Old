using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public static class RepositoryPatternExtensions
    {
        public static IServiceCollection AddRepositoryPattern(this IServiceCollection services)
        {
            services.AddSwaggerGen();
            return services;
        }
        public static IApplicationBuilder UseRepositoryPattern(this IApplicationBuilder app, string swaggerEndpoint = "/swagger/v1/swagger.json", string apiVersion = "Api V1")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(swaggerEndpoint, apiVersion);
            });
            return app;
        }
    }
}
