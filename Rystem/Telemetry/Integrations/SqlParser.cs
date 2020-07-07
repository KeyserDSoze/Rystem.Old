using Microsoft.Extensions.Logging;
using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class SqlParser : IAggregationParser<Telemetry>
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        public SqlParser(TelemetryConfiguration telemetryConfiguration)
            => this.TelemetryConfiguration = telemetryConfiguration;
        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            throw new NotImplementedException();
        }
    }
}
