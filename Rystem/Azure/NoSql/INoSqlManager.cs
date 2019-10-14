using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal interface INoSqlManager
    {
        Task<bool> ExistsAsync(INoSql entity, Installation installation);
        Task<IList<TNoSqlEntity>> GetAsync<TNoSqlEntity>(INoSql entity, Installation installation, Expression<Func<TNoSqlEntity, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSql;
        Task<bool> UpdateAsync(INoSql entity, Installation installation);
        Task<bool> DeleteAsync(INoSql entity, Installation installation);
        string GetName(Installation installation);
    }
}
