using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Rystem
{
    public class TraceTelemetry : ITelemetryEvent
    {
        [JsonIgnore]
        public Telemetry Telemetry { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}