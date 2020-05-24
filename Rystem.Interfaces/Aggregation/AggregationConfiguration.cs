using System;
using System.Collections.Generic;

namespace Rystem.Aggregation
{
    public class AggregationConfiguration : IConfiguration
    {
        public string Name { get; set; }
        public int MaximumBuffer { get; set; } = 10000;
        public long MaximumTime { get; set; } = TimeSpan.FromMinutes(5).Ticks;
        public IList<IAggregationParser> Parsers { get; set; } = new List<IAggregationParser>();
    }
}
