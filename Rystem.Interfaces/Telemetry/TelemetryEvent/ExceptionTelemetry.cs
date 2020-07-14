using Newtonsoft.Json;
using System;

namespace Rystem
{
    public class ExceptionTelemetry : ITelemetryEvent, IRystemTelemetryEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        private Exception exception;
        [JsonIgnore]
        public Exception Exception
        {
            get => exception;
            set
            {
                exception = value;
                StackTrace = value.StackTrace;
                HResult = value.HResult;
            }
        }
        public string StackTrace { get; set; }
        public int HResult { get; set; }
    }
}