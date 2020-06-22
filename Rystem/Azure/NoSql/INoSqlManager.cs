
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.NoSql
{
    internal interface INoSqlManager<TEntity> : IRystemManager<TEntity>
    {
        Task<bool> ExistsAsync(TEntity entity, Installation installation);
        Task<IList<TEntity>> GetAsync(TEntity entity, Installation installation, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null);
        Task<bool> UpdateAsync(TEntity entity, Installation installation);
        Task<bool> UpdateBatchAsync(IEnumerable<TEntity> entity, Installation installation);
        Task<bool> DeleteAsync(TEntity entity, Installation installation);
        Task<bool> DeleteBatchAsync(IEnumerable<TEntity> entities, Installation installation);
        string GetName(Installation installation);
    }
}
