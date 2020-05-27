using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class RystemCacheConfiguration : IConfiguration
    {
        public CacheConsistency Consistency { get; internal set; }
        public string ConnectionString { get; internal set; }
        public bool HasMemory => MemoryProperties != null;
        public bool HasCloud => CloudProperties != null;
        public CacheConfiguration MemoryProperties { get; internal set; }
        public CacheConfiguration CloudProperties { get; internal set; }
        
    }
}