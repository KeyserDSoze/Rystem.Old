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
    internal class AppendBlobParser : IAggregationParser<Telemetry>
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        private readonly ConfigurationBuilder BaseConfigurationForData;
        public AppendBlobParser(TelemetryConfiguration telemetryConfiguration, Installation installation)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            if (this.TelemetryConfiguration.ObjectName == null)
                this.TelemetryConfiguration.ObjectName = "telemetry";
            this.BaseConfigurationForData = new ConfigurationBuilder()
            .WithData(telemetryConfiguration.ConnectionString)
            .WithAppendBlobStorage(new Data.AppendBlobBuilder(telemetryConfiguration.Name, new StringBlobManager(), new StringBlobManager()))
            .Build(installation);
        }

        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            List<Task> writes = new List<Task>();
            foreach (var telemetry in events.GroupBy(x => x.Key))
            {
                writes.Add(new TelemetryData()
                {
                    ConfigurationBuilder = BaseConfigurationForData,
                    Properties = new DataProperties() { ContentType = "text/avro" },
                    Name = $"{this.TelemetryConfiguration.ObjectName}/{(telemetry.Key == null ? string.Empty : $"{telemetry.Key}/")}{DateTime.UtcNow:yyyyMMddHH}.avro",
                    Events = telemetry
                }.WriteAsync(installation: installation));
            }
            await Task.WhenAll(writes).NoContext();
        }
    }
    internal class TelemetryData : IData
    {
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
        public IEnumerable<Telemetry> Events { get; set; }
        public ConfigurationBuilder ConfigurationBuilder { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder()
            => this.ConfigurationBuilder;
    }
    internal class StringBlobManager : IDataWriter<TelemetryData>, IDataReader<TelemetryData>
    {
        public async Task<WrapperEntity<TelemetryData>> ReadAsync(DataWrapper dummy)
        {
            List<Telemetry> telemetries = new List<Telemetry>();
            using StreamReader reader = new StreamReader(dummy.Stream);
            while (!reader.EndOfStream)
                telemetries.Add((await reader.ReadLineAsync()).FromDefaultJson<Telemetry>());
            return new WrapperEntity<TelemetryData>()
            {
                Entities = new List<TelemetryData> { new TelemetryData { Name = dummy.Name, Properties = dummy.Properties, Events = telemetries } }
            };
        }

        public Task<DataWrapper> WriteAsync(TelemetryData entity)
        {
            return Task.FromResult(new DataWrapper()
            {
                Properties = entity.Properties,
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes($"{string.Join('\n', entity.Events.Select(x => x.ToDefaultJson()))}{'\n'}"))
            });
        }
    }
}