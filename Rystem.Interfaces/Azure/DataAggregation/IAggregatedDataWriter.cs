using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IDataLakeWriter
    {
        AggregatedDataDummy Write(IAggregatedData entity);
    }
}
