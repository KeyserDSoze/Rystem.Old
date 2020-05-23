using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class RystemCacheProperty
    {
        public CacheConsistency Consistency { get; }
        public string ConnectionString { get; }
        public bool HasMemory { get; }
        public bool HasCloud { get; }
        public CacheProperties MemoryProperties { get; }
        public CacheProperties CloudProperties { get; }
        public RystemCacheProperty(CacheConsistency consistency, string connectionString, CacheProperties memoryProperties, CacheProperties cloudProperties)
        {
            this.Consistency = consistency;
            this.ConnectionString = connectionString;
            this.HasMemory = memoryProperties != null;
            this.HasCloud = cloudProperties != null;
            this.MemoryProperties = memoryProperties;
            this.CloudProperties = cloudProperties;
        }
    }
}