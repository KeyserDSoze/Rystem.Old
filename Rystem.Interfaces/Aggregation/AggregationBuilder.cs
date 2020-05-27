using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationBuilder<T> : IInstallingBuilder
    {
        internal readonly IConfiguration AggregationConfiguration;
        private readonly AggregationSelector<T> AggregationSelector;
        internal AggregationBuilder(IConfiguration aggregationConfiguration, AggregationSelector<T> aggregationSelector)
        {
            this.AggregationConfiguration = aggregationConfiguration;
            this.AggregationSelector = aggregationSelector;
        }
        public InstallerType InstallerType => InstallerType.Aggregation;

        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.AggregationSelector.Builder.AddConfiguration(this.AggregationConfiguration, this.InstallerType, installation);
            return this.AggregationSelector.Builder;
        }
    }
}
