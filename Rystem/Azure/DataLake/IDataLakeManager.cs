using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.DataLake
{
    internal interface IDataLakeManager
    {
        Task<bool> ExistsAsync(IDataLake entity);
        Task<IDataLake> FetchAsync(IDataLake entity);
        Task<string> WriteAsync(IDataLake entity);
        Task<bool> AppendAsync(IDataLake entity);
        Task<bool> DeleteAsync(IDataLake entity);
        Task<IList<IDataLake>> ListAsync(IDataLake entity, string prefix = null, int? takeCount = null);
        Task<IList<string>> SearchAsync(IDataLake entity, string prefix = null, int? takeCount = null);
    }
}
