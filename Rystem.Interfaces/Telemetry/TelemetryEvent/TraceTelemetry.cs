using Microsoft.Extensions.Logging;
using System;

namespace Rystem
{
    public class TraceTelemetry : ITelemetryEvent, IRystemTelemetryEvent
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}