using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class BlobStorageCacheProperties
    {
        public CacheConfiguration Properties { get; }
        public BlobStorageCacheProperties(ExpireTime expireTime, bool consistency = false) : this((int)expireTime, consistency)
        {
        }
        public BlobStorageCacheProperties(int expireSeconds, bool consistency = false)
        {
            this.Properties = new CacheConfiguration()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                Type = CloudCacheType.BlobStorage
            };
        }
    }
}
