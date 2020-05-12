using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal interface IMultitonManager<TCache>
        where TCache : IMultiton, new()
    {
        Task<TCache> InstanceAsync(IMultitonKey<TCache> key);
        Task<bool> UpdateAsync(IMultitonKey<TCache> key, TCache value, TimeSpan expiringTime);
        Task<bool> ExistsAsync(IMultiKey key);
        Task<bool> DeleteAsync(IMultiKey key);
        Task<IEnumerable<string>> ListAsync();
    }
}
