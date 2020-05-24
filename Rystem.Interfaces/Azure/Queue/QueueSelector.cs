using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class QueueSelector
    {
        private readonly string ConnectionString;
        internal readonly Installer AzureInstaller;
        internal QueueSelector(string connectionString, Installer azureInstaller)
        {
            this.ConnectionString = connectionString;
            this.AzureInstaller = azureInstaller;
        }
        public QueueBuilder WithServiceBus(ServiceBusBuilder serviceBusBuilder)
        {
            serviceBusBuilder.QueueConfiguration.ConnectionString = this.ConnectionString;
            return new QueueBuilder(serviceBusBuilder.QueueConfiguration, this);
        }
        public QueueBuilder WithEventHub(RijndaelBuilder eventHubBuilder)
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
