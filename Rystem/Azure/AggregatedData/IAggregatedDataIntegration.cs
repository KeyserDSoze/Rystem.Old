using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    internal interface IAggregatedDataIntegration<TEntity>
        where TEntity : IAggregatedData
    {
        Task<bool> ExistsAsync(IAggregatedData entity);
        Task<bool> WriteAsync(IAggregatedData entity, long offset);
        Task<TEntity> FetchAsync(IAggregatedData entity);
        Task<bool> DeleteAsync(IAggregatedData entity);
        Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix, int? takeCount);
        Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix, int? takeCount);
    }
}
