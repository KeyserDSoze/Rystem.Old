using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedDataWriter<TEntity>
        where TEntity : IAggregatedData
    {
        Task<AggregatedDataDummy> WriteAsync(TEntity entity);
    }
}
