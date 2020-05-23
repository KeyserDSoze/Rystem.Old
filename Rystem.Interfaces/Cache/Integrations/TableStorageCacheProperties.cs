using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class TableStorageCacheProperties
    {
        public CacheProperties Properties { get; }
        public TableStorageCacheProperties(ExpireTime expireTime, bool consistency = false) : this((int)expireTime, consistency)
        {
        }
        public TableStorageCacheProperties(int expireSeconds, bool consistency = false)
        {
            this.Properties = new CacheProperties()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                Type = CloudCacheType.TableStorage
            };
        }
    }
}
