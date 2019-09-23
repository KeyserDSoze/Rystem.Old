using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedData
    {
        string Name { get; set; }
        AggregatedDataProperties Properties { get; set; }
    }
}
