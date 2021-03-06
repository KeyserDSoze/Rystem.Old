using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class AppendBlobTelemetry : IAggregationParser<Telemetry>, ITelemetryIntegration
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        private readonly ConfigurationBuilder BaseConfigurationForData;
        private readonly ConfigurationBuilder BaseConfigurationForEventData;
        private readonly Installation Installation;
        public AppendBlobTelemetry(TelemetryConfiguration telemetryConfiguration, Installation installation)
        {
            this.Installation = installation;
            this.TelemetryConfiguration = telemetryConfiguration;
            if (this.TelemetryConfiguration.ObjectName == null)
                this.TelemetryConfiguration.ObjectName = "telemetry";
            this.BaseConfigurationForData = new ConfigurationBuilder()
            .WithData(telemetryConfiguration.ConnectionString)
            .WithAppendBlobStorage(new Data.AppendBlobBuilder(telemetryConfiguration.Name, new StringBlobManager<TelemetryData, Telemetry>(), new StringBlobManager<TelemetryData, Telemetry>()))
            .Build(installation);
            this.BaseConfigurationForEventData = new ConfigurationBuilder()
            .WithData(telemetryConfiguration.ConnectionString)
            .WithAppendBlobStorage(new Data.AppendBlobBuilder(telemetryConfiguration.Name, new StringBlobManager<TelemetryEventData, ITelemetryEvent>(), new StringBlobManager<TelemetryEventData, ITelemetryEvent>()))
            .Build(installation);
        }

        public async Task<IEnumerable<Telemetry>> GetEventsAsync(DateTime from, DateTime to, string key)
        {
            List<Telemetry> telemetries = new List<Telemetry>();
            try
            {
                await foreach (var telemetryData in GetTelemetryDataAsync())
                    if (telemetryData != null)
                        telemetries.AddRange(telemetryData.Events);
            }
            catch (Exception ex)
            {
                string olaf = ex.ToString();
            }
            return telemetries;

            async IAsyncEnumerable<TelemetryData> GetTelemetryDataAsync()
            {
                while (from <= to)
                {
                    string name = $"{this.TelemetryConfiguration.ObjectName}/{(key == null ? string.Empty : $"{key}/")}{from:yyyyMMddHH}.avro";
                    if (await new TelemetryData()
                    {
                        ConfigurationBuilder = BaseConfigurationForData,
                        Name = name
                    }.ExistsAsync())
                        yield return (await new TelemetryData()
                        {
                            ConfigurationBuilder = BaseConfigurationForData,
                        }.ListAsync(name, null, this.Installation).NoContext()).FirstOrDefault();
                    from = from.AddHours(1);
                }
            }
        }

        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            List<Task> writes = new List<Task>();
            foreach (var telemetryByDate in events.GroupBy(x => x.Start.ToString("yyyyMMddHH")))
            {
                foreach (var telemetry in telemetryByDate.GroupBy(x => x.Key))
                {
                    writes.Add(new TelemetryData()
                    {
                        ConfigurationBuilder = BaseConfigurationForData,
                        Properties = new DataProperties() { ContentType = "text/avro" },
                        Name = $"{this.TelemetryConfiguration.ObjectName}/{(telemetry.Key == null ? string.Empty : $"{telemetry.Key}/")}{telemetryByDate.Key}.avro",
                        Events = telemetry
                    }.WriteAsync(installation: installation));
                    foreach (var telemetryByEvent in telemetry.SelectMany(x => x.Events).GroupBy(x => x.GetType().Name))
                    {
                        writes.Add(new TelemetryEventData()
                        {
                            ConfigurationBuilder = BaseConfigurationForEventData,
                            Properties = new DataProperties() { ContentType = "text/avro" },
                            Name = $"{this.TelemetryConfiguration.ObjectName}/{(telemetry.Key == null ? string.Empty : $"{telemetry.Key}/")}{telemetryByEvent.Key}/{telemetryByDate.Key}.avro",
                            Events = telemetryByEvent
                        }.WriteAsync(installation: installation));
                    }
                }
            }
            await Task.WhenAll(writes).NoContext();
        }
    }
}