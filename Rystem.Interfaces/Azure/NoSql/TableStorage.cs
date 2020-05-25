using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public abstract class TableStorage : INoSql
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
        public abstract ConfigurationBuilder GetConfigurationBuilder();
    }
}
