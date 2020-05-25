using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastCacheInstaller
    {
        public static CacheBuilder Properties { get; private set; } = new CacheBuilder().WithMemory(new MemoryCacheProperties(ExpireTime.OneDay));
        public static void Configure(CacheBuilder builder)
            => Properties = builder;
    }
}
