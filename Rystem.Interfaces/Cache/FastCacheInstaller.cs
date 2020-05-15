using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class FastCacheInstaller
    {
        public static MultitonProperties Properties;
        public static void Configure(MultitonProperties properties) 
            => Properties = properties;
    }
}
