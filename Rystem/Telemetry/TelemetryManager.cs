using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class TelemetryManager : ITelemetryManager
    {
        private readonly IDictionary<Installation, TelemetryConfiguration> TelemetryConfiguration;
        private readonly TelemetryAggregator Aggregator;
        public TelemetryManager(ConfigurationBuilder configurationBuilder)
        {
            this.TelemetryConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as TelemetryConfiguration);
            ConfigurationBuilder aggregatorConfigurationBuilder = new ConfigurationBuilder();
            foreach (var configuration in TelemetryConfiguration)
            {
                var aggregationConfiguration = configuration.Value.AggregationConfiguration;
                aggregatorConfigurationBuilder.WithAggregation<Telemetry>()
                .WithLinq(new LinqBuilder<Telemetry>(aggregationConfiguration.Name, aggregationConfiguration.MaximumBuffer, new TimeSpan(aggregationConfiguration.MaximumTime)))
                .AddParser(GetParser(configuration.Value, configuration.Key))
                .Build(configuration.Key);
            }
            this.Aggregator = new TelemetryAggregator()
            {
                Configuration = aggregatorConfigurationBuilder
            };

            IAggregationParser<Telemetry> GetParser(TelemetryConfiguration telemetryConfiguration, Installation installation)
            {
                switch (telemetryConfiguration.Type)
                {
                    case TelemetryType.AppendBlob:
                        return new AppendBlobParser(telemetryConfiguration, installation);
                    case TelemetryType.Sql:
                        return new SqlParser(telemetryConfiguration);
                    default:
                        throw new InvalidOperationException($"Wrong type installed {telemetryConfiguration.Type}");
                }
            }
        }

        public InstallerType InstallerType => InstallerType.Telemetry;

        public async Task TrackEvent(Telemetry telemetry, Installation installation)
            => await this.Aggregator.RunAsync(telemetry, installation: installation);
    }
}