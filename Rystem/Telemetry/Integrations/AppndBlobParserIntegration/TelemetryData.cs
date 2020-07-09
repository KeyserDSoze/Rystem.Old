using Rystem.Data;
using System.Collections.Generic;

namespace Rystem
{
    internal class TelemetryData : ITelemetryData<Telemetry>, IData
    {
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
        public IEnumerable<Telemetry> Events { get; set; }
        public ConfigurationBuilder ConfigurationBuilder { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
            => this.ConfigurationBuilder;
    }
}