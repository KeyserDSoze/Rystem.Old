using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationSelector<T>
    {
        internal readonly Installer Installer;
        internal AggregationSelector(Installer installer)
            => this.Installer = installer;
        public AggregationParser<T> WithLinq(LinqBuilder<T> linqBuilder)
            => new AggregationParser<T>(this, new AggregationBuilder<T>((linqBuilder ?? new LinqBuilder<T>(string.Empty)).AggregationConfiguration, this));
    }
}
