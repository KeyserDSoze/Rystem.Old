using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal interface ICacheManager : IRystemManager { }
    internal interface ICacheManager<TCacheKey, TCache> : ICacheManager
        where TCacheKey : ICacheKey<TCache>
    {
        Task<TCache> InstanceAsync(TCacheKey key);
        Task<bool> UpdateAsync(TCacheKey key, TCache value, TimeSpan expiringTime);
        Task<bool> ExistsAsync(TCacheKey key);
        Task<bool> DeleteAsync(TCacheKey key);
        Task<IEnumerable<string>> ListAsync();
        Task WarmUp();
    }
}
