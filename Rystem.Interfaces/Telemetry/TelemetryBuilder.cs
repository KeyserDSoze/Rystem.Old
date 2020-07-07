using System;

namespace Rystem
{
    public class TelemetryBuilder : IInstallingBuilder
    {
        internal readonly IConfiguration TelemetryConfiguration;
        private readonly TelemetrySelector TelemetrySelector;
        internal TelemetryBuilder(IConfiguration telemetryConfiguration, TelemetrySelector telemetrySelector)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            this.TelemetrySelector = telemetrySelector;
        }
        public InstallerType InstallerType => InstallerType.Telemetry;

        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.TelemetrySelector.Builder.AddConfiguration(this.TelemetryConfiguration, this.InstallerType, installation);
            return this.TelemetrySelector.Builder;
        }
    }
}