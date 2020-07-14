using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;

namespace Rystem
{
    public class DependencyTelemetry : ITelemetryEvent, IRystemTelemetryEvent
    {
        public string Name { get; set; }
        public string Caller { get; set; }
        public string PathCaller { get; set; }
        public int LineNumberCaller { get; set; }
        public bool Success { get; set; }
        public string Response { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public long Elapsed { get; set; }
        [JsonIgnore]
        public Stopwatch Timer { get; set; } = Stopwatch.StartNew();
        [JsonIgnore]
        public Action<DependencyTelemetry> StopAction { get; set; }

        public void Stop()
        {
            this.Timer.Stop();
            this.Elapsed = this.Timer.Elapsed.Ticks;
            StopAction(this);
        }
    }
}