using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public abstract class FastTableStorage : TableStorage
    {
        public override ConfigurationBuilder GetConfigurationBuilder() 
            => FastNoSqlInstaller.Builder;
    }
}
