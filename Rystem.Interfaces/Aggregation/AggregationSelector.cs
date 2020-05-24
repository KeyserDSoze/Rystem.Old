using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Aggregation
{
    public class AggregationSelector
    {
        internal readonly Installer Installer;
        internal AggregationSelector(Installer installer)
            => this.Installer = installer;
        public AggregationParser WithLinq(LinqBuilder linqBuilder)
            => new AggregationParser(this, new AggregationBuilder((linqBuilder ?? new LinqBuilder(string.Empty)).AggregationConfiguration, this));
    }
}
