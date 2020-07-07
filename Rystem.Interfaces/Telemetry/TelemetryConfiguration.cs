using Rystem.Aggregation;
using System;
using System.Collections.Generic;

namespace Rystem
{
    public class TelemetryConfiguration : IConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public TelemetryType Type { get; set; }
        public AggregationConfiguration<Telemetry> AggregationConfiguration { get; set; }
    }
}