using Rystem.Azure.NoSql;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastInstaller
    {
        public static void ConfigureMultiton(CacheBuilder builder)
            => FastCacheInstaller.Configure(builder);
        public static void ConfigureNoSql(ConfigurationBuilder configurationBuilder)
            => FastNoSqlInstaller.Configure(configurationBuilder);
    }
}
