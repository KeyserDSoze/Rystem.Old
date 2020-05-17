using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Rystem.Web
{
    public class RequestContext
    {
        [JsonIgnore]
        public string Action { get; set; }
        [JsonIgnore]
        public string Controller { get; set; }
        [JsonIgnore]
        public string Area { get; set; }
        [JsonIgnore]
        public string FurtherPath { get; set; }
        [JsonIgnore]
        public object Model { get; set; }
        [JsonIgnore]
        public Dictionary<object, object> QueryString { get; set; }
        [JsonIgnore]
        public RequestType RequestType { get; set; }
        [JsonProperty("method")]
        public string Method => this.RequestType.ToString().ToLower();
        [JsonProperty("onSuccess")]
        public string SuccessCallback { get; set; }
        [JsonProperty("onFailure")]
        public string FailureCallback { get; set; }
        [JsonProperty("selector")]
        public string Selector { get; set; }
        [JsonProperty("url")]
        public string Url { get; private set; }
        [JsonProperty("data")]
        public string Data => this.Model.ToDefaultJson();
        internal string FinalizeRequestContext(HttpRequest request)
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
                else
                    throw new ArgumentException("Context was not recognized a valid controller, please use asp-controller attribute in ajax-modal tag.");
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
            this.Url = builder.ToString();
            return this.ToDefaultJson();
        }
    }
}
