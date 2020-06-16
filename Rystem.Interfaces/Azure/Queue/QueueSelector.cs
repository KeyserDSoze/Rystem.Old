using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Queue
{
    public class QueueSelector : IBuildingSelector
    {
        private readonly string ConnectionString;
        public ConfigurationBuilder Builder { get; }

        internal QueueSelector(string connectionString, ConfigurationBuilder builder)
        {
            this.ConnectionString = connectionString;
            this.Builder = builder;
        }
        public QueueBuilder WithServiceBus(ServiceBusBuilder serviceBusBuilder)
        {
            serviceBusBuilder.QueueConfiguration.ConnectionString = this.ConnectionString;
            return new QueueBuilder(serviceBusBuilder.QueueConfiguration, this);
        }
        public QueueBuilder WithEventHub(EventHubBuilder eventHubBuilder)
        {
            eventHubBuilder.QueueConfiguration.ConnectionString = this.ConnectionString;
            return new QueueBuilder(eventHubBuilder.QueueConfiguration, this);
        }
        public QueueBuilder WithQueueStorage(QueueStorageBuilder queueStorageBuilder)
        {
            queueStorageBuilder.QueueConfiguration.ConnectionString = this.ConnectionString;
            return new QueueBuilder(queueStorageBuilder.QueueConfiguration, this);
        }
        public QueueBuilder WithSmartQueue(SmartQueueBuilder smartQueueBuilder)
        {
            smartQueueBuilder.QueueConfiguration.ConnectionString = this.ConnectionString;
            return new QueueBuilder(smartQueueBuilder.QueueConfiguration, this);
        }
    }
}
