using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public sealed class AggregatedDataDummy
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
