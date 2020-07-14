using System;

namespace Rystem
{
    public class MetricTelemetry : ITelemetryEvent, IRystemTelemetryEvent
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}