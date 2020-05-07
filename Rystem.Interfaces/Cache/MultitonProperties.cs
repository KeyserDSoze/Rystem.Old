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
        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, ExpireTime expireTime, int numberOfClients = 10) : this(connectionString, cloudType, (int)expireTime, numberOfClients)
        {
        }

        public InCloudMultitonProperties(string connectionString, InCloudType cloudType, int secondsToExpire, int numberOfClients = 10) : base(secondsToExpire)
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
        public TimeSpan ExpireTimeSpan => TimeSpan.FromSeconds(this.ExpireSeconds);
        public ExpiringProperties(ExpireTime expireTime)
            => this.ExpireSeconds = (int)expireTime;
        public ExpiringProperties(int secondsToExpire)
            => ExpireSeconds = secondsToExpire;
    }
}
