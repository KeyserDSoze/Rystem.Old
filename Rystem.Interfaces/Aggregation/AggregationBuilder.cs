using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationBuilder
    {
        internal readonly IConfiguration AggregationConfiguration;
        private readonly AggregationSelector AggregationSelector;
        public AggregationBuilder(IConfiguration aggregationConfiguration, AggregationSelector aggregationSelector)
        {
            this.AggregationConfiguration = aggregationConfiguration;
            this.AggregationSelector = aggregationSelector;
        }
        public ConfigurationBuilder Build()
        {
            this.AggregationSelector.Installer.AddConfiguration(this.AggregationConfiguration);
            return this.AggregationSelector.Installer.Builder;
        }
    }
}
