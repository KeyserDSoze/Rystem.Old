using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal interface INoSqlIntegration<TEntity>
        where TEntity : INoSql
    {
        Task<bool> ExistsAsync(TEntity entity);
        Task<IList<TSpecialEntity>> GetAsync<TSpecialEntity>(TEntity entity, Expression<Func<TSpecialEntity, bool>> expression = null, int? takeCount = null)
            where TSpecialEntity : INoSql;
        Task<bool> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TEntity entity);
        Task<bool> UpdateBatchAsync(IEnumerable<TEntity> entities);
        Task<bool> DeleteBatchAsync(IEnumerable<TEntity> entities);
    }
}
