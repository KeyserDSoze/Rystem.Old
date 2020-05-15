using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class MultitonProperties
    {
        public InCloudMultitonProperties InCloudProperties { get; }
        public ExpiringProperties InMemoryProperties { get; }
        public CacheConsistency Consistency { get; }
        public MultitonProperties(InCloudMultitonProperties inCloudProperties, ExpiringProperties inMemoryPropeperties, CacheConsistency consistency)
        {
            this.InCloudProperties = inCloudProperties;
            this.InMemoryProperties = inMemoryPropeperties;
            this.Consistency = consistency;
        }
        public MultitonProperties(InCloudMultitonProperties inCloudProperties, CacheConsistency consistency) : this(inCloudProperties, null, consistency) { }
        public MultitonProperties(ExpiringProperties inMemoryPropeperties, CacheConsistency consistency) : this(null, inMemoryPropeperties, consistency) { }
    }
    public class InCloudMultitonProperties : ExpiringProperties
    {
        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, ExpireTime expireTime, int numberOfClients = 5, bool garbageCollection = false) : this(connectionString, cloudType, (int)expireTime, numberOfClients, garbageCollection)
        {
        }

        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, int secondsToExpire, int numberOfClients = 5, bool garbageCollection = false) : base(secondsToExpire, false, garbageCollection)
        {
            this.ConnectionString = connectionString;
            this.CloudType = cloudType;
            this.NumberOfClients = numberOfClients;
        }

        public string ConnectionString { get; }
        public InCloudType CloudType { get; }
        public int NumberOfClients { get; }
    }
    public class ExpiringProperties
    {
        public int ExpireSeconds { get; }
        public bool Consistency { get; }
        public bool GarbageCollection { get; }
        public TimeSpan ExpireTimeSpan
            => TimeSpan.FromSeconds(this.ExpireSeconds);
        public ExpiringProperties(ExpireTime expireTime, bool consistency = false, bool garbageCollection = false) : this((int)expireTime, consistency, garbageCollection) { }
        public ExpiringProperties(int secondsToExpire, bool consistency, bool garbageCollection)
        {
            ExpireSeconds = secondsToExpire;
            Consistency = consistency;
            this.GarbageCollection = garbageCollection;
        }
    }
}
