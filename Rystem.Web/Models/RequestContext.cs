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
            if (this.Area != null)
                builder.Append($"/{this.Area}");
            if (this.Controller != null)
                builder.Append($"/{this.Controller}");
            else
            {
                string[] splittingPath = request.Path.Value.Split('/');
                if (splittingPath.Length >= 2 && !string.IsNullOrWhiteSpace(splittingPath[1]))
                    builder.Append($"/{splittingPath[1]}");
                else
                    throw new ArgumentException("Context was not recognized a valid controller, please use asp-controller attribute in ajax-modal tag.");
            }
            builder.Append($"/{this.Action}");
            if (this.QueryString != null && this.QueryString.Count > 0)
                builder.Append($"?{string.Join("&", this.QueryString.Select(x => $"{x.Key}={x.Value}"))}");
            this.Url = builder.ToString();
            return this.ToDefaultJson();
        }
    }
}
