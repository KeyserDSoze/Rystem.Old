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
        Task<bool> ExistsAsync(INoSql entity);
        Task<IList<TEntity>> GetAsync(INoSql entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null);
        Task<bool> UpdateAsync(INoSql entity);
        Task<bool> DeleteAsync(INoSql entity);
    }
}
