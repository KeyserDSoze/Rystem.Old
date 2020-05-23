using Rystem.Azure.NoSql;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public class FastNoSqlInstaller
    {
        public static NoSqlConfiguration Properties;
        public static void Configure(NoSqlConfiguration properties) 
            => Properties = properties;
    }
}
