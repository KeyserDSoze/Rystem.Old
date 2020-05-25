using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CloudCacheBuilder
    {
        private readonly CacheBuilder CacheBuilder;
        internal CloudCacheBuilder(CacheBuilder cacheBuilder)
            => this.CacheBuilder = cacheBuilder;
        public CacheBuilder WithRedis(RedisCacheProperties configuration)
        {
            this.CacheBuilder.CloudProperties = configuration.Properties;
            return this.CacheBuilder;
        }
        public CacheBuilder WithTablestorage(TableStorageCacheProperties configuration)
        {
            this.CacheBuilder.CloudProperties = configuration.Properties;
            return this.CacheBuilder;
        }
        public CacheBuilder WithBlobstorage(BlobStorageCacheProperties configuration)
        {
            this.CacheBuilder.CloudProperties = configuration.Properties;
            return this.CacheBuilder;
        }
    }
}
