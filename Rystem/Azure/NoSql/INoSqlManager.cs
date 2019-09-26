using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal interface INoSqlManager
    {
        Task<bool> ExistsAsync(INoSql entity);
        Task<IEnumerable<TNoSqlEntity>> FetchAsync<TNoSqlEntity>(INoSql entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSql;
        Task<bool> UpdateAsync(INoSql entity);
        Task<bool> DeleteAsync(INoSql entity);
    }
}
