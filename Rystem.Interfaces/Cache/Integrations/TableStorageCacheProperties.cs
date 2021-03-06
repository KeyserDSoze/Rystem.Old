using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class TableStorageCacheProperties
    {
        public CacheConfiguration Properties { get; }
        public TableStorageCacheProperties(ExpireTime expireTime, bool consistency = false, string @namespace = null) : this((int)expireTime, consistency, @namespace)
        {
        }
        public TableStorageCacheProperties(int expireSeconds, bool consistency = false, string @namespace = null)
        {
            this.Properties = new CacheConfiguration()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                Type = CloudCacheType.TableStorage,
                Namespace = @namespace
            };
        }
    }
}
