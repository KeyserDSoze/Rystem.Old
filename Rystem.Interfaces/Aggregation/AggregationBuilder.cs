using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationBuilder<T> : IBuilder
    {
        internal readonly IConfiguration AggregationConfiguration;
        private readonly AggregationSelector<T> AggregationSelector;
        internal AggregationBuilder(IConfiguration aggregationConfiguration, AggregationSelector<T> aggregationSelector)
        {
            this.AggregationConfiguration = aggregationConfiguration;
            this.AggregationSelector = aggregationSelector;
        }

        public InstallerType InstallerType => InstallerType.Aggregation;

        public ConfigurationBuilder Build()
        {
            this.AggregationSelector.Installer.AddConfiguration(this.AggregationConfiguration, this.InstallerType);
            return this.AggregationSelector.Installer.Builder;
        }
    }
}
