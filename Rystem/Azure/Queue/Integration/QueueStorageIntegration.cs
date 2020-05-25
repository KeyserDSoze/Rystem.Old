using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class QueueStorageIntegration<TEntity> : IQueueIntegration<TEntity>
    {
        private static readonly object TrafficLight = new object();
        private CloudQueue client;
        private CloudQueue Client
        {
            get
            {
                if (client != null)
                    return client;
                lock (TrafficLight)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(QueueConfiguration.ConnectionString);
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                    client = queueClient.GetQueueReference(QueueConfiguration.Name ?? typeof(TEntity).Name);
                }
                try { client.CreateIfNotExistsAsync().ToResult(); } catch { }
                return client;
            }
        }
        private readonly QueueConfiguration QueueConfiguration;
        internal QueueStorageIntegration(QueueConfiguration property) 
            => this.QueueConfiguration = property;

        public Task<bool> CleanAsync()
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");

        public Task<bool> DeleteScheduledAsync(long messageId)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");

        public async Task<IEnumerable<TEntity>> ReadAsync(int path, int organization)
             => (await this.Client.PeekMessagesAsync(this.QueueConfiguration.NumberOfMessages).NoContext()).Select(x => x.AsString.ToMessage<TEntity>());
        public async Task<bool> SendAsync(TEntity message, int path, int organization)
        {
            await this.Client.AddMessageAsync(new CloudQueueMessage(message.ToDefaultJson())).NoContext();
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
        {
            await this.Client.AddMessageAsync(new CloudQueueMessage(messages.ToDefaultJson())).NoContext();
            return true;
        }

        public Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");

        public Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");
    }
}
