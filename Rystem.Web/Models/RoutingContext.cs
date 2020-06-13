using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web
{
    public class RoutingContext
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public string FurtherPath { get; set; }
        public Dictionary<object, object> QueryString { get; set; }
        public string GetUrl(ViewContext viewContext)
        {
            if (this.Area == null && viewContext.RouteData.Values.ContainsKey("area"))
                this.Area = viewContext.RouteData.Values["area"].ToString();
            if (this.Controller == null && viewContext.RouteData.Values.ContainsKey("controller"))
                this.Controller = viewContext.RouteData.Values["controller"].ToString();
            if (this.Action == null && viewContext.RouteData.Values.ContainsKey("action"))
                this.Action = viewContext.RouteData.Values["action"].ToString();

            return this.GetUrl(viewContext.HttpContext.Request);
        }
        public string GetUrl(HttpRequest request)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{request.Scheme}://{request.Host}");
            string[] splittingPath = request.Path.Value.Split('/');
            if (this.Area != null)
                builder.Append($"/{this.Area}");
            if (this.Controller != null)
                builder.Append($"/{this.Controller}");
            else
            {
                if (this.Area == null && splittingPath.Length > 1 && !string.IsNullOrWhiteSpace(splittingPath[1]))
                    builder.Append($"/{splittingPath[1]}");
                else if (this.Area != null && splittingPath.Length > 2 && !string.IsNullOrWhiteSpace(splittingPath[2]))
                    builder.Append($"/{splittingPath[2]}");
            }

            if (this.Action != null)
                builder.Append($"/{this.Action}");
            else if (this.Area == null && splittingPath.Length > 2)
                builder.Append($"/{splittingPath[2]}");
            else if (this.Area != null && splittingPath.Length > 3)
                builder.Append($"/{splittingPath[3]}");

            if (this.FurtherPath != null)
                builder.Append($"/{this.FurtherPath}");
            else if (this.Area == null && splittingPath.Length > 3)
                builder.Append($"/{string.Join("/", splittingPath.Skip(3))}");
            else if (this.Area != null && splittingPath.Length > 4)
                builder.Append($"/{string.Join("/", splittingPath.Skip(4))}");

            if (this.QueryString != null && this.QueryString.Count > 0)
                builder.Append($"?{string.Join("&", this.QueryString.Select(x => $"{x.Key}={x.Value}"))}");
            return builder.ToString();
        }
    }
}