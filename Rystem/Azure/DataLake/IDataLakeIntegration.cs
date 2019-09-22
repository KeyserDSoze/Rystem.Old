using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.DataLake
{
    internal interface IDataLakeIntegration
    {
        Task<bool> ExistsAsync(IDataLake entity);
        Task<IDataLake> FetchAsync(IDataLake entity);
        Task<bool> UpdateAsync(IDataLake entity);
        Task<bool> DeleteAsync(IDataLake entity);
        Task<IList<IDataLake>> ListAsync(IDataLake entity);
        Task<IList<string>> SearchAsync(IDataLake entity);
    }
}
