using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class AppendBlobTelemetryBuilder
    {
        public TelemetryConfiguration TelemetryConfiguration { get; }
        public AppendBlobTelemetryBuilder(string name, string blobSubDirectoryName = null)
            => this.TelemetryConfiguration = new TelemetryConfiguration()
            {
                Name = name,
                ObjectName = blobSubDirectoryName,
                Type = TelemetryType.AppendBlob,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = name,
                }
            };
        public AppendBlobTelemetryBuilder(string name, int maximumBuffer, TimeSpan maximumTime, string blobSubDirectoryName = null)
            => this.TelemetryConfiguration = new TelemetryConfiguration()
            {
                Name = name,
                ObjectName = blobSubDirectoryName,
                Type = TelemetryType.AppendBlob,
                AggregationConfiguration = new AggregationConfiguration<Telemetry>()
                {
                    Name = name,
                    MaximumBuffer = maximumBuffer,
                    MaximumTime = maximumTime.Ticks
                }
            };
    }
}