using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class SqlTelemetryBuilder
    {
        public TelemetryConfiguration TelemetryConfiguration { get; }
        public SqlTelemetryBuilder(string aggregationName, string tableName = null, string tableObjectName = null)
            => this.TelemetryConfiguration = new TelemetryConfiguration()
            {
                Name = tableName,
                ObjectName = tableObjectName,
                Type = TelemetryType.Sql,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = aggregationName,
                }
            };
        public SqlTelemetryBuilder(string name, int maximumBuffer, TimeSpan maximumTime, string tableName = null, string tableObjectName = null)
            => this.TelemetryConfiguration = new TelemetryConfiguration()
            {
                Name = tableName,
                ObjectName = tableObjectName,
                Type = TelemetryType.Sql,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = name,
                    MaximumBuffer = maximumBuffer,
                    MaximumTime = maximumTime.Ticks
                }
            };
    }
}