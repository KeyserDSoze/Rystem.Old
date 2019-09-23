using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal interface INoSqlIntegration<TEntity>
        where TEntity : INoSqlStorage
    {
        Task<bool> ExistsAsync(INoSqlStorage entity);
        Task<IList<TEntity>> GetAsync(INoSqlStorage entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null);
        Task<bool> UpdateAsync(INoSqlStorage entity);
        Task<bool> DeleteAsync(INoSqlStorage entity);
    }
}
