﻿using System;

namespace Rystem.Azure.Storage
{
    public class NoTableStorageProperty : Attribute { }
    public interface INoSqlStorage
    {
       
    }
    public interface ITableStorage : INoSqlStorage
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTime Timestamp { get; set; }
        string ETag { get; set; }
    }
}
