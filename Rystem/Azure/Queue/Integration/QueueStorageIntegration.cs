using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Queue
{
    internal class QueueStorageIntegration<TEntity> : IQueueIntegration<TEntity>
    {
        private static readonly object TrafficLight = new object();
        private QueueClient context;
        private QueueClient Context
        {
            get
            {
                if (context != null)
                    return context;
                lock (TrafficLight)
                {
                    var client = new QueueServiceClient(QueueConfiguration.ConnectionString);
                    context = client.GetQueueClient(QueueConfiguration.Name ?? typeof(TEntity).Name);
                }
                if (!context.ExistsAsync().ToResult())
                    context.CreateIfNotExistsAsync();
                return context;
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
        {
            var messages = (await this.Context.ReceiveMessagesAsync(this.QueueConfiguration.NumberOfMessages).NoContext()).Value;
            List<TEntity> entities = new List<TEntity>();
            foreach (var message in messages)
            {
                await this.Context.DeleteMessageAsync(message.MessageId, message.PopReceipt).NoContext();
                entities.Add(message.MessageText.ToMessage<TEntity>());
            }
            return entities;
        }

        public async Task<bool> SendAsync(TEntity message, int path, int organization)
            => !string.IsNullOrWhiteSpace((await this.Context.SendMessageAsync(message.ToDefaultJson()).NoContext()).Value.MessageId);

        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
        => !string.IsNullOrWhiteSpace((await this.Context.SendMessageAsync(messages.ToDefaultJson()).NoContext()).Value.MessageId);

        public Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");

        public Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");
    }
}
