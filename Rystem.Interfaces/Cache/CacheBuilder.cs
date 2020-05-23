using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CacheBuilder
    {
        internal CacheConsistency Consistency { get; private set; }
        internal string ConnectionString { get; private set; }
        internal CacheProperties MemoryProperties { get; private set; }
        internal CacheProperties CloudProperties { get; set; }
        public RystemCacheProperty Property
            => new RystemCacheProperty(this.Consistency, this.ConnectionString, this.MemoryProperties, this.CloudProperties);
        public CacheBuilder() : this(CacheConsistency.Always)
        {

        }
        public CacheBuilder(CacheConsistency consistency)
        {
            this.Consistency = consistency;
        }
        public CacheBuilder WithMemory(MemoryCacheProperties properties)
        {
            this.MemoryProperties = properties.Properties;
            return this;
        }
        public CloudCacheBuilder WithCloud(string connectionString)
        {
            this.ConnectionString = connectionString;
            return new CloudCacheBuilder(this);
        }
    }
}
