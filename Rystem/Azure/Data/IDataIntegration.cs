using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Data
{
    internal interface IDataIntegration<TEntity>
        where TEntity : IData
    {
        Task<bool> ExistsAsync(TEntity entity);
        Task<bool> WriteAsync(TEntity entity, long offset);
        Task<TEntity> FetchAsync(TEntity entity);
        Task<bool> DeleteAsync(TEntity entity);
        Task<IList<TEntity>> ListAsync(TEntity entity, string prefix, int? takeCount);
        Task<IList<string>> SearchAsync(TEntity entity, string prefix, int? takeCount);
        Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, string prefix, int? takeCount);
    }
}
