using System;

namespace Rystem
{
    public class ExceptionTelemetry : ITelemetryEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Exception Exception { get; set; }
    }
}