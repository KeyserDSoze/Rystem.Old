using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class NoMultitonKey : Attribute { }
    /// <summary>
    /// Interface to implement Multiton logic. Please don't use in your string key this value '╬'
    /// </summary>
    /// <typeparam name="TCache"></typeparam>
    public interface ICacheKey<TCache>
    {
        CacheBuilder CacheBuilder();
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        Task<TCache> FetchAsync();
    }
}
