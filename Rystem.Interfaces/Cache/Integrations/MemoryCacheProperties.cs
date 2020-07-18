using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class MemoryCacheProperties
    {
        public CacheConfiguration Properties { get; }
        public MemoryCacheProperties(ExpireTime expireTime, bool consistency = false, bool garbageCollection = false, string @namespace = null) : this((int)expireTime, consistency, garbageCollection, @namespace)
        {
        }
        public MemoryCacheProperties(int expireSeconds, bool consistency = false, bool garbageCollection = false, string @namespace = null)
        {
            this.Properties = new CacheConfiguration()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                GarbageCollection = garbageCollection,
                Namespace = @namespace
            };
        }
    }
}
