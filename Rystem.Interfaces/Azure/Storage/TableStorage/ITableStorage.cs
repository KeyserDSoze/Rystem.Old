﻿using System;

namespace Rystem.Azure.Storage
{
    public interface ITableStorage
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
        DateTimeOffset Timestamp { get; set; }
        string ETag { get; set; }
    }
}