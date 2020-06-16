using Rystem.NoSql;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastInstaller
    {
        public static void ConfigureMultiton(ConfigurationBuilder builder)
            => FastCacheInstaller.Configure(builder);
        public static void ConfigureTableStorage(string connectionString)
            => FastNoSqlInstaller.Configure(new ConfigurationBuilder()
                .WithNoSql(connectionString)
                .WithTableStorage(new TableStorageBuilder())
                .Build());
    }
}
