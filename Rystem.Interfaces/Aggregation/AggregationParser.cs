using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationParser<T>
    {
        private readonly AggregationBuilder<T> AggregationBuilder;
        internal AggregationParser(AggregationBuilder<T> aggregationBuilder) 
            => this.AggregationBuilder = aggregationBuilder;
        public AggregationParser<T> AddParser(IAggregationParser<T> parser)
        {
            (this.AggregationBuilder.AggregationConfiguration as AggregationConfiguration<T>).Parsers.Add(parser);
            return this;
        }
        public ConfigurationBuilder Build(Installation installation = Installation.Default)
            => this.AggregationBuilder.Build(installation);
    }
}
