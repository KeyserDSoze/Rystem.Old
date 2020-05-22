
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal interface IDataManager
    {
        Task<bool> ExistsAsync(IData entity, Installation installation);
        Task<TEntityLake> FetchAsync<TEntityLake>(IData entity, Installation installation)
            where TEntityLake : IData;
        Task<bool> WriteAsync(IData entity, Installation installation, long offset);
        Task<bool> DeleteAsync(IData entity, Installation installation);
        Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IData entity, Installation installation, string prefix = null, int? takeCount = null)
            where TEntityLake : IData;
        Task<IList<string>> SearchAsync(IData entity, Installation installation, string prefix = null, int? takeCount = null);
        Task<IList<DataWrapper>> FetchPropertiesAsync(IData entity, Installation installation, string prefix = null, int? takeCount = null);
        string GetName(Installation installation);
    }
}
