using Rystem.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public abstract class SuperFastTableStorage : TableStorage
    {
        [NoSqlProperty]
        protected abstract string ConnectionString { get; }
        public override ConfigurationBuilder GetConfigurationBuilder()
            => new ConfigurationBuilder()
                .WithNoSql(this.ConnectionString)
                .WithTableStorage(new TableStorageBuilder(this.GetType().Name))
                .Build();
    }
}