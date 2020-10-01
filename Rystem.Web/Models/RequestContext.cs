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
        [JsonProperty("onRedirect")]
        public bool OnRedirect { get; set; }
        [JsonProperty("url")]
        public string Url { get; private set; }
        [JsonProperty("data")]
        public string Data => this.Model.ToDefaultJson();
        internal string FinalizeRequestContext(HttpRequest request)
        {
            this.Url = new RoutingContext()
            {
                Action = this.Action,
                Area = this.Area,
                Controller = this.Controller,
                FurtherPath = this.FurtherPath,
                QueryString = this.QueryString
            }.GetUrl(request);
            return this.ToJsonNoNull();
        }
    }
}
