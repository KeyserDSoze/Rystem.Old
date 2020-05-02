using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    public interface IAggregatedDataReader<TEntity>
        where TEntity : IAggregatedData, new()
    {
        Task<TEntity> ReadAsync(AggregatedDataDummy dummy);
    }
    public interface IAggregatedDataListReader<TEntity>
        where TEntity : IAggregatedData, new()
    {
        Task<IList<TEntity>> ReadAsync(AggregatedDataDummy dummy);
    }
}
