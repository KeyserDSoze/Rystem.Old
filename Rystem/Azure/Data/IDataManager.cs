using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Data
{
    internal interface IDataManager<TEntity> : IManager<TEntity>
        where TEntity : IData
    {
        Task<bool> ExistsAsync(TEntity entity, Installation installation);
        Task<TEntity> FetchAsync(TEntity entity, Installation installation);
        Task<bool> WriteAsync(TEntity entity, Installation installation, long offset);
        Task<bool> DeleteAsync(TEntity entity, Installation installation);
        Task<IEnumerable<TEntity>> ListAsync(TEntity entity, Installation installation, string prefix = null, int? takeCount = null);
        Task<IList<string>> SearchAsync(TEntity entity, Installation installation, string prefix = null, int? takeCount = null);
        Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, Installation installation, string prefix = null, int? takeCount = null);
        string GetName(Installation installation);
    }
}
