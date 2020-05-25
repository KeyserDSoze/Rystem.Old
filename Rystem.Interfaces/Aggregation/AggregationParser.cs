using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationParser<T>
    {
        private readonly AggregationSelector<T> AggregationSelector;
        private readonly AggregationBuilder<T> AggregationBuilder;
        public AggregationParser(AggregationSelector<T> aggregationSelector, AggregationBuilder<T> aggregationBuilder)
        {
            this.AggregationBuilder = aggregationBuilder;
            this.AggregationSelector = aggregationSelector;
        }
        public AggregationParser<T> AddParser(IAggregationParser<T> parser)
        {
            (this.AggregationBuilder.AggregationConfiguration as AggregationConfiguration<T>).Parsers.Add(parser);
            return this;
        }
        public AggregationBuilder<T> Configure()
            => this.AggregationBuilder;
    }
}
