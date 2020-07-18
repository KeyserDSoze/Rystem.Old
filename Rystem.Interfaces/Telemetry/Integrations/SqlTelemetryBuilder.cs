using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class SqlTelemetryBuilder
    {
        public SqlTelemetryConfiguration TelemetryConfiguration { get; }
        public SqlTelemetryBuilder(string tableName, string tableObjectName = null)
            => this.TelemetryConfiguration = new SqlTelemetryConfiguration()
            {
                Name = tableName,
                ObjectName = tableObjectName,
                Type = TelemetryType.Sql,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = tableName,
                }
            };
        public SqlTelemetryBuilder(string tableName, int maximumBuffer, TimeSpan maximumTime, string tableObjectName = null)
            => this.TelemetryConfiguration = new SqlTelemetryConfiguration()
            {
                Name = tableName,
                ObjectName = tableObjectName,
                Type = TelemetryType.Sql,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = tableName,
                    MaximumBuffer = maximumBuffer,
                    MaximumTime = maximumTime.Ticks
                }
            };
    }
}