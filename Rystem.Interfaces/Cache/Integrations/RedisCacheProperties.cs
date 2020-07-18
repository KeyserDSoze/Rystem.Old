using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class RedisCacheProperties
    {
        public CacheConfiguration Properties { get; }
        public RedisCacheProperties(ExpireTime expireTime, bool consistency = false, int numberOfClients = 5, string @namespace = null) : this((int)expireTime, consistency, numberOfClients, @namespace)
        {
        }
        public RedisCacheProperties(int expireSeconds, bool consistency = false, int numberOfClients = 5, string @namespace = null)
        {
            this.Properties = new CacheConfiguration()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                NumberOfClients = numberOfClients,
                Type = CloudCacheType.RedisCache,
                Namespace = @namespace
            };
        }
    }
}
