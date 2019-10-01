using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public interface ITableStorage : INoSql
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTime Timestamp { get; set; }
        string ETag { get; set; }
    }
}
