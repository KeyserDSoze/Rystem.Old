using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web.Backoffice
{
    public static class BackofficeExtensions
    {
        public static void Backoffice(this IApplicationBuilder app)
        {
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.Map("/", async context =>
            //    {
            //        await context.BackofficeAsync();
            //    });
            //});
        }
    }
}
