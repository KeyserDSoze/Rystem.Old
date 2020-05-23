using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class RedisCacheProperties
    {
        public CacheProperties Properties { get; }
        public RedisCacheProperties(ExpireTime expireTime, bool consistency = false, int numberOfClients = 5) : this((int)expireTime, consistency, numberOfClients)
        {
        }
        public RedisCacheProperties(int expireSeconds, bool consistency = false, int numberOfClients = 5)
        {
            this.Properties = new CacheProperties()
            {
                ExpireSeconds = expireSeconds,
                Consistency = consistency,
                NumberOfClients = numberOfClients,
                Type = CloudCacheType.RedisCache
            };
        }
    }
}
