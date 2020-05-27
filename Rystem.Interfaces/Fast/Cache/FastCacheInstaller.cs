using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastCacheInstaller
    {
        public static ConfigurationBuilder Properties { get; private set; } = new ConfigurationBuilder().WithCache().WithMemory(new MemoryCacheProperties(ExpireTime.OneDay)).Build();
        public static void Configure(ConfigurationBuilder builder)
            => Properties = builder;
    }
}
