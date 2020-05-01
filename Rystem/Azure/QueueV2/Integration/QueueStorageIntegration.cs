using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
#warning Manca tutta la parte di gestione della coda, di lettura dei messaggi e di quanti ce ne sono
    internal class QueueStorageIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        private CloudQueue Client;
        internal QueueStorageIntegration(QueueConfiguration property)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(property.ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            this.Client = queueClient.GetQueueReference(property.Name);
            this.Client.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task<bool> CleanAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            await Task.Delay(0);
            throw new NotImplementedException("Storage queue doesn't allow this operation.");
        }

        public Task<IList<TEntity>> Read(int path, int organization)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> SendAsync(IQueue message, int path, int organization)
        {
            await this.Client.AddMessageAsync(new CloudQueueMessage(message.ToJson()));
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization)
        {
            await this.Client.AddMessageAsync(new CloudQueueMessage(messages.ToJson()));
            return true;
        }

        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization)
        {
            await Task.Delay(0);
            throw new NotImplementedException("Storage queue doesn't allow this operation.");
        }

        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            await Task.Delay(0);
            throw new NotImplementedException("Storage queue doesn't allow this operation.");
        }
    }
}
