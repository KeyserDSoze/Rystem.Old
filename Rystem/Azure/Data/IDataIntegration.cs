using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal interface IDataIntegration<TEntity>
        where TEntity : IData
    {
        Task<bool> ExistsAsync(IData entity);
        Task<bool> WriteAsync(IData entity, long offset);
        Task<TEntity> FetchAsync(IData entity);
        Task<bool> DeleteAsync(IData entity);
        Task<IList<TEntity>> ListAsync(IData entity, string prefix, int? takeCount);
        Task<IList<string>> SearchAsync(IData entity, string prefix, int? takeCount);
        Task<IList<DataWrapper>> FetchPropertiesAsync(IData entity, string prefix, int? takeCount);
    }
}
