using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal class FastCacheKey : IMultitonKey<FastCache>
    {
        static FastCacheKey()
            => MultitonInstaller.Configure<FastCacheKey, FastCache>(FastCacheInstaller.Properties);
        public string Key { get; set; }
        public Task<FastCache> FetchAsync()
            => Task.FromResult(default(FastCache));
    }
    internal class FastCache : IMultiton
    {
        public string Value { get; set; }
    }
}
