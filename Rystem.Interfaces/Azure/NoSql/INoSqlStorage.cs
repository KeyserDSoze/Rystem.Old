using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public interface INoSql
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTime Timestamp { get; set; }
        string Tag { get; set; }
    }
}
