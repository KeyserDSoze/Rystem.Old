using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class NoMultitonKey : Attribute { }
    public interface IMultitonKey<TCache> : IMulti, IMultiKey
        where TCache : IMultiton
    {
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        TCache Fetch();
    }
}
