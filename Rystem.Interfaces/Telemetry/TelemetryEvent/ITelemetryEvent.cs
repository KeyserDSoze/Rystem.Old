using Newtonsoft.Json;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public interface ITelemetryEvent
    {
        public string Id { get; set; }
        public string TelemetryId { get; set; }
        [JsonIgnore]
        Telemetry Telemetry { get; set; }
        DateTime Timestamp { get; set; }
    }
}