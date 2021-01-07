using Rystem.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Fast
{
    public abstract class FastBlobStorage : BlobStorage
    {
        public override ConfigurationBuilder GetConfigurationBuilder()
            => FastNoSqlInstaller.Builder;
    }
}
