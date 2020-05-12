using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    public class MultitonProperties
    {
        public InCloudMultitonProperties InCloudProperties { get; }
        public ExpiringProperties InMemoryProperties { get; }
        public MultitonProperties(InCloudMultitonProperties inCloudProperties, ExpiringProperties inMemoryPropeperties)
        {
            this.InCloudProperties = inCloudProperties;
            this.InMemoryProperties = inMemoryPropeperties;
        }
        public MultitonProperties(InCloudMultitonProperties inCloudProperties)
            => this.InCloudProperties = inCloudProperties;
        public MultitonProperties(ExpiringProperties inMemoryPropeperties)
            => this.InMemoryProperties = inMemoryPropeperties;
    }
    public class InCloudMultitonProperties : ExpiringProperties
    {
        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, ExpireTime expireTime, int numberOfClients = 5) : this(connectionString, cloudType, (int)expireTime, numberOfClients)
        {
        }

        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, int secondsToExpire, int numberOfClients = 5) : base(secondsToExpire, false)
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
        public TimeSpan ExpireTimeSpan
            => TimeSpan.FromSeconds(this.ExpireSeconds);
        public ExpiringProperties(ExpireTime expireTime, bool consistency = false) : this((int)expireTime, consistency) { }
        public ExpiringProperties(int secondsToExpire, bool consistency)
        {
            ExpireSeconds = secondsToExpire;
            Consistency = consistency;
        }
    }
}
