using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class TelemetryEventData : ITelemetryData<ITelemetryEvent>, IData
    {
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
        public IEnumerable<ITelemetryEvent> Events { get; set; }
        public ConfigurationBuilder ConfigurationBuilder { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
            => this.ConfigurationBuilder;
    }
}