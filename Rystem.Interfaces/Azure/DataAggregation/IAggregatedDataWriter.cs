using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedDataWriter<TEntity>
        where TEntity : IAggregatedData
    {
        AggregatedDataDummy Write(TEntity entity);
    }
}
