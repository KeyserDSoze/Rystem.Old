using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationParser
    {
        private readonly AggregationSelector AggregationSelector;
        private readonly AggregationBuilder AggregationBuilder;
        public AggregationParser(AggregationSelector aggregationSelector, AggregationBuilder aggregationBuilder)
        {
            this.AggregationBuilder = aggregationBuilder;
            this.AggregationSelector = aggregationSelector;
        }
        public AggregationParser AddParser(IAggregationParser parser)
        {
            (this.AggregationBuilder.AggregationConfiguration as AggregationConfiguration).Parsers.Add(parser);
            return this;
        }
        public AggregationBuilder Configure()
            => this.AggregationBuilder;
    }
}
