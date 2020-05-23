using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal interface IMultitonIntegrationAsync<T> 
    {
        Task<T> InstanceAsync(string key);
        Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime);
        Task<MultitonStatus<T>> ExistsAsync(string key);
        Task<bool> DeleteAsync(string key);
        Task<IEnumerable<string>> ListAsync();
        Task WarmUp();
    }
    internal interface IMultitonIntegration<T>
    {
        T Instance(string key);
        bool Update(string key, T value, TimeSpan expiringTime);
        MultitonStatus<T> Exists(string key);
        bool Delete(string key);
        IEnumerable<string> List();
    }
}
