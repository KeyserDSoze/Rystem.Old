using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class CacheProperties
    {
        public int ExpireSeconds { get; set; }
        public bool Consistency { get; set; }
        public bool GarbageCollection { get; set; }
        public int NumberOfClients { get; set; }
        public CloudCacheType Type { get; internal set; }
        public TimeSpan ExpireTimeSpan
            => TimeSpan.FromSeconds(this.ExpireSeconds);
    }
}
