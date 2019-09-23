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
        Task<string> WriteAsync(IAggregatedData entity);
        Task<TEntity> FetchAsync(IAggregatedData entity);
        Task<bool> AppendAsync(IAggregatedData entity, long offset = 0);
        Task<bool> DeleteAsync(IAggregatedData entity);
        Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix = null, int? takeCount = null);
        Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null);
    }
}
