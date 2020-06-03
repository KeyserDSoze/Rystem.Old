using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Routing;

namespace Rystem.Web
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseRystem(this IApplicationBuilder builder)
        {
            var embeddedProvider = new EmbeddedFileProvider(typeof(ApplicationBuilderExtensions)
                .GetTypeInfo().Assembly, "Rystem.Web.wwwroot");

            builder.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = embeddedProvider,
                RequestPath = new PathString("/rystem")
            });

            //builder.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "Rystem",
            //        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
