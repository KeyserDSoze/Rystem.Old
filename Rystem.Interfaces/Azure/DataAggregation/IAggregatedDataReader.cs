using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedDataReader<TEntity>
    {
        TEntity Read(AggregatedDataDummy dummy);
    }
    public interface IAggregatedDataListReader<TEntity>
    {
        IList<TEntity> Read(AggregatedDataDummy dummy);
    }
}
