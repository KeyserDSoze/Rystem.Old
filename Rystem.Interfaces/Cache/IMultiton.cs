using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public interface IMultiton : IMulti
    {
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        IMultiton Fetch(IMultitonKey key);
    }
    public interface IMulti { }
}
