using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CloudCacheBuilder
    {
        private readonly CacheSelector CacheSelector;
        internal CloudCacheBuilder(CacheSelector cacheSelector)
            => this.CacheSelector = cacheSelector;
        public CacheBuilder WithRedis(RedisCacheProperties configuration)
        {
            this.CacheSelector.Configuration.CloudProperties = configuration.Properties;
            return new CacheBuilder(this.CacheSelector.Configuration, this.CacheSelector);
        }
        public CacheBuilder WithTablestorage(TableStorageCacheProperties configuration)
        {
            this.CacheSelector.Configuration.CloudProperties = configuration.Properties;
            return new CacheBuilder(this.CacheSelector.Configuration, this.CacheSelector);
        }
        public CacheBuilder WithBlobstorage(BlobStorageCacheProperties configuration)
        {
            this.CacheSelector.Configuration.CloudProperties = configuration.Properties;
            return new CacheBuilder(this.CacheSelector.Configuration, this.CacheSelector);
        }
    }
}
