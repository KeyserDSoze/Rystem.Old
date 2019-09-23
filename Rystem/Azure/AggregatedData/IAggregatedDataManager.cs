using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    internal interface IAggregatedDataManager
    {
        Task<bool> ExistsAsync(IAggregatedData entity);
        Task<TEntityLake> FetchAsync<TEntityLake>(IAggregatedData entity)
            where TEntityLake : IAggregatedData;
        Task<string> WriteAsync(IAggregatedData entity);
        Task<bool> AppendAsync(IAggregatedData entity, long offset = 0);
        Task<bool> DeleteAsync(IAggregatedData entity);
        Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IAggregatedData entity, string prefix = null, int? takeCount = null)
            where TEntityLake : IAggregatedData;
        Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null);
    }
}
