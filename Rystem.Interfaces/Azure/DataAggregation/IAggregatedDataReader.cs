using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedDataReader<TEntity>
        where TEntity : IAggregatedData
    {
        TEntity Read(AggregatedDataDummy dummy);
    }
    public interface IAggregatedDataListReader<TEntity>
        where TEntity : IAggregatedData
    {
        IList<TEntity> Read(AggregatedDataDummy dummy);
    }
}
