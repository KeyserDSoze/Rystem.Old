using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CacheSelector : IBuildingSelector
    {
        public RystemCacheConfiguration Configuration { get; } = new RystemCacheConfiguration();

        public ConfigurationBuilder Builder { get; }

        internal CacheSelector(ConfigurationBuilder builder) : this(builder, CacheConsistency.Always)
        {
        }
        public CacheSelector(ConfigurationBuilder builder, CacheConsistency consistency)
        {
            this.Builder = builder;
            this.Configuration.Consistency = consistency;
        }
        public CacheBuilder WithMemory(MemoryCacheProperties properties)
        {
            this.Configuration.MemoryProperties = properties.Properties;
            return new CacheBuilder(this.Configuration, this);
        }
        public CloudCacheBuilder WithCloud(string connectionString)
        {
            this.Configuration.ConnectionString = connectionString;
            return new CloudCacheBuilder(this);
        }
    }
}
