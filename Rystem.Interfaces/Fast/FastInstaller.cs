using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastInstaller
    {
        public static void ConfigureMultiton(MultitonProperties properties)
            => FastCacheInstaller.Configure(properties);
    }
}
