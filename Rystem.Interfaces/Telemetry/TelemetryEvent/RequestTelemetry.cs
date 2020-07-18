using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class RequestTelemetry : ITelemetryEvent
    {
        [JsonIgnore]
        public Telemetry Telemetry { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
        public long BodyLength { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Method { get; set; }
        public string RequestUri { get; set; }
        public string Version { get; set; }
    }
}
