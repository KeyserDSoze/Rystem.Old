using Rystem.Azure.NoSql;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    internal class FastNoSqlInstaller
    {
        public static ConfigurationBuilder Builder { get; private set; }
        public static void Configure(ConfigurationBuilder configurationBuilder)
            => Builder = configurationBuilder;
    }
}
