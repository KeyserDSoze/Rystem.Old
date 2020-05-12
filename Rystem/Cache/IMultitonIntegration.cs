using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal interface IMultitonIntegration<T> 
        where T : IMultiton, new()
    {
        Task<T> InstanceAsync(string key);
        Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime);
        Task<bool> ExistsAsync(string key);
        Task<bool> DeleteAsync(string key);
        Task<IEnumerable<string>> ListAsync();
    }
}
