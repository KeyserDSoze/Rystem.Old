using System;

namespace Rystem.Azure.NoSql
{
    public class NoTableStorageProperty : Attribute { }
    public interface ITableStorage : INoSqlStorage
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTime Timestamp { get; set; }
        string ETag { get; set; }
    }
}
