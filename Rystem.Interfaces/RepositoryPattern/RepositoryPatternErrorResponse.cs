using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Rystem;

namespace System
{
    public class RepositoryPatternErrorResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("traceId")]
        public string TraceId { get; set; }
    }
}