using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Rystem.Web
{
    public static class ApplicationBuilderExtensions
    {
        private class AreaViewLocationExpander : IViewLocationExpander
        {
            public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
                => viewLocations.Concat(new[] { "/Views/Shared/{0}" + RazorViewEngine.ViewExtension });

            public virtual void PopulateValues(ViewLocationExpanderContext context)
            {
            }
        }
        public static void AddRystemViews(this IServiceCollection services)
        {
            var assembly = typeof(AreaViewLocationExpander).GetTypeInfo().Assembly;
            //Create an EmbeddedFileProvider for that assembly
            var embeddedFileProvider = new EmbeddedFileProvider(
                assembly,
                "ViewComponentLibrary"
            );
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new AreaViewLocationExpander());
            });
            services.AddMvc()
                .ConfigureApplicationPartManager(partManager =>
                {
                    var thisAssembly = typeof(AreaViewLocationExpander).Assembly;
                    var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(thisAssembly, throwOnError: true);
                    var relatedParts = relatedAssemblies.ToDictionary(
                        ra => ra,
                        CompiledRazorAssemblyApplicationPartFactory.GetDefaultApplicationParts);
                    foreach (var kvp in relatedParts)
                    {
                        var assemblyName = kvp.Key.GetName().Name;
                        foreach (var t in kvp.Value)
                            partManager.ApplicationParts.Add(t);
                    }
                    Assembly GetApplicationAssembly()
                    {
                        // Whis is the same logic that MVC follows to find the application assembly.
                        var environment = services.Where(d => d.ServiceType == typeof(IWebHostEnvironment)).ToArray();
                        var applicationName = ((IWebHostEnvironment)environment.LastOrDefault()?.ImplementationInstance)
                            .ApplicationName;

                        var appAssembly = Assembly.Load(applicationName);
                        return appAssembly;
                    }
                });
        }

        public static IApplicationBuilder UseRystem(this IApplicationBuilder builder)
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
            return builder;
        }
    }
}