using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class LinqBuilder<T>
    {
        public AggregationConfiguration<T> AggregationConfiguration { get; }
        public LinqBuilder(string name)
            => this.AggregationConfiguration = new AggregationConfiguration<T>() { Name = name };
        public LinqBuilder(string name, int maximumBuffer, TimeSpan maximumTime)
            => this.AggregationConfiguration = new AggregationConfiguration<T>()
            {
                Name = name,
                MaximumBuffer = maximumBuffer,
                MaximumTime = maximumTime.Ticks
            };
    }
}
