using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class NoMultitonKey : Attribute { }
    [Obsolete("This interface will be removed in future version. Please use IMultitonKey<TCache> instead of IMultitonKey.")]
    public interface IMultitonKey : IMulti, IMultiKey
    {
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        IMultiton Fetch();
    }
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
