using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class BlobStorageCacheProperties
    {
        public CacheProperties Properties { get; }
        public BlobStorageCacheProperties(ExpireTime expireTime, bool consistency = false) : this((int)expireTime, consistency)
        {
        }
        public BlobStorageCacheProperties(int expireSeconds, bool consistency = false)
        {
            this.Properties = new CacheProperties()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                Type = CloudCacheType.BlobStorage
            };
        }
    }
}
