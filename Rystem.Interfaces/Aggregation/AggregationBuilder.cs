using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationBuilder<T>
    {
        internal readonly IConfiguration AggregationConfiguration;
        private readonly AggregationSelector<T> AggregationSelector;
        internal AggregationBuilder(IConfiguration aggregationConfiguration, AggregationSelector<T> aggregationSelector)
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
