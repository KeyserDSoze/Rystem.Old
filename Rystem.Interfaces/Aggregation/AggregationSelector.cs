using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationSelector<T> : IBuildingSelector
    {
        public ConfigurationBuilder Builder { get; }
        internal AggregationSelector(ConfigurationBuilder builder)
            => this.Builder = builder;

        public AggregationParser<T> WithLinq(LinqBuilder<T> linqBuilder)
            => new AggregationParser<T>(new AggregationBuilder<T>((linqBuilder ?? new LinqBuilder<T>(string.Empty)).AggregationConfiguration, this));
    }
}
