using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal interface INoSqlManager
    {
        Task<bool> ExistsAsync(INoSqlStorage entity);
        Task<IEnumerable<TNoSqlEntity>> FetchAsync<TNoSqlEntity>(INoSqlStorage entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSqlStorage;
        Task<bool> UpdateAsync(INoSqlStorage entity);
        Task<bool> DeleteAsync(INoSqlStorage entity);
    }
}
