using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public enum CacheConsistency
    {
        /// <summary>
        /// Always allows to lock the execution during retrieve information for your cache. Use this with caring.
        /// </summary>
        Always,
        /// <summary>
        /// Cache engine executes contemporaneity with contemporary retrieves and contemporary updates in cache. Use this with caring.
        /// </summary>
        Never
    }
}
