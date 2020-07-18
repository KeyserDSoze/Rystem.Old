using Rystem.Aggregation;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;

namespace Rystem
{
    public class SqlTelemetryConfiguration : TelemetryConfiguration
    {
        public Dictionary<string, SqlTable> CustomTables { get; set; } = new Dictionary<string, SqlTable>();
    }
    public class TelemetryConfiguration : IConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public TelemetryType Type { get; set; }
        public AggregationConfiguration<Telemetry> AggregationConfiguration { get; set; }
    }
}