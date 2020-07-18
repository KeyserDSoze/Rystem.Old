using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class TelemetryManager : ITelemetryManager
    {
        private readonly IDictionary<Installation, TelemetryConfiguration> TelemetryConfiguration;
        private readonly TelemetryAggregator Aggregator;
        private readonly IDictionary<Installation, ITelemetryIntegration> Integrations = new Dictionary<Installation, ITelemetryIntegration>();
        public TelemetryManager(ConfigurationBuilder configurationBuilder)
        {
            this.TelemetryConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as TelemetryConfiguration);
            ConfigurationBuilder aggregatorConfigurationBuilder = new ConfigurationBuilder();
            foreach (var configuration in TelemetryConfiguration)
            {
                var integration = GetIntegration(configuration.Value, configuration.Key);
                this.Integrations.Add(configuration.Key, integration);
                var aggregationConfiguration = configuration.Value.AggregationConfiguration;
                aggregatorConfigurationBuilder.WithAggregation<Telemetry>()
                .WithLinq(new LinqBuilder<Telemetry>(aggregationConfiguration.Name, aggregationConfiguration.MaximumBuffer, new TimeSpan(aggregationConfiguration.MaximumTime)))
                .AddParser(integration as IAggregationParser<Telemetry>)
                .Build(configuration.Key);
            }
            this.Aggregator = new TelemetryAggregator()
            {
                Configuration = aggregatorConfigurationBuilder,
            };

            ITelemetryIntegration GetIntegration(TelemetryConfiguration telemetryConfiguration, Installation installation)
            {
                switch (telemetryConfiguration.Type)
                {
                    case TelemetryType.AppendBlob:
                        return new AppendBlobTelemetry(telemetryConfiguration, installation);
                    case TelemetryType.Sql:
                        return new SqlTelemetry(telemetryConfiguration as SqlTelemetryConfiguration);
                    default:
                        throw new InvalidOperationException($"Wrong type installed {telemetryConfiguration.Type}");
                }
            }
        }

        public InstallerType InstallerType => InstallerType.Telemetry;

        public async Task<IEnumerable<Telemetry>> GetEventsAsync(Expression<Func<Telemetry, bool>> expression, Installation installation) 
            => await this.Integrations[installation].GetEventsAsync(expression).NoContext();

        public async Task TrackEventAsync(Telemetry telemetry, Installation installation)
            => await this.Aggregator.RunAsync(telemetry, installation: installation);
    }
}