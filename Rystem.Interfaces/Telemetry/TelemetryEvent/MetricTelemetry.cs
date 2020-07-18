using Newtonsoft.Json;
using System;

namespace Rystem
{
    public class MetricTelemetry : ITelemetryEvent
    {
        [JsonIgnore]
        public Telemetry Telemetry { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}