using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class MemoryCacheProperties
    {
        public CacheProperties Properties { get; }
        public MemoryCacheProperties(ExpireTime expireTime, bool consistency = false, bool garbageCollection = false) : this((int)expireTime, consistency, garbageCollection)
        {
        }
        public MemoryCacheProperties(int expireSeconds, bool consistency = false, bool garbageCollection = false)
        {
            this.Properties = new CacheProperties()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                GarbageCollection = garbageCollection
            };
        }
    }
}
