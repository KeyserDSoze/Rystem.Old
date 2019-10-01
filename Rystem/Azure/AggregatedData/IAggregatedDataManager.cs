using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    internal interface IAggregatedDataManager
    {
        Task<bool> ExistsAsync(IAggregatedData entity, Installation installation);
        Task<TEntityLake> FetchAsync<TEntityLake>(IAggregatedData entity, Installation installation)
            where TEntityLake : IAggregatedData;
        Task<string> WriteAsync(IAggregatedData entity, Installation installation);
        Task<bool> AppendAsync(IAggregatedData entity, Installation installation, long offset = 0);
        Task<bool> DeleteAsync(IAggregatedData entity, Installation installation);
        Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IAggregatedData entity, Installation installation, string prefix = null, int? takeCount = null)
            where TEntityLake : IAggregatedData;
        Task<IList<string>> SearchAsync(IAggregatedData entity, Installation installation, string prefix = null, int? takeCount = null);
        string GetName(Installation installation);
    }
}
