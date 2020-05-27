using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rystem.Cache;

namespace Rystem.Fast
{
    internal class FastCacheKey : ICacheKey<FastCache>
    {
        public string Key { get; set; }
        public ConfigurationBuilder GetConfigurationBuilder() 
            => FastCacheInstaller.Properties;

        public Task<FastCache> FetchAsync()
            => Task.FromResult(default(FastCache));
    }
    internal class FastCache
    {
        public string Value { get; set; }
    }
}
