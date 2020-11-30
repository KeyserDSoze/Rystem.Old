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
        private readonly RaceCondition RaceCondition = new RaceCondition();
        private QueueClient context;
        private async Task<QueueClient> GetContextAsync()
        {
            if (context == null)
                await RaceCondition.ExecuteAsync(async () =>
                {
                    if (context == null)
                    {
                        var client = new QueueServiceClient(QueueConfiguration.ConnectionString);
                        var preContext = client.GetQueueClient(QueueConfiguration.Name ?? typeof(TEntity).Name);
                        if (!await preContext.ExistsAsync().NoContext())
                            await preContext.CreateIfNotExistsAsync().NoContext();
                        context = preContext;
                    }
                }).NoContext();
            return context;
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
            var client = context ?? await GetContextAsync();
            var messages = (await client.ReceiveMessagesAsync(this.QueueConfiguration.NumberOfMessages).NoContext()).Value;
            List<TEntity> entities = new List<TEntity>();
            foreach (var message in messages)
            {
                await client.DeleteMessageAsync(message.MessageId, message.PopReceipt).NoContext();
                entities.Add(message.MessageText.ToMessage<TEntity>());
            }
            return entities;
        }

        public async Task<bool> SendAsync(TEntity message, int path, int organization)
        {
            var client = context ?? await GetContextAsync();
            return !string.IsNullOrWhiteSpace((await client.SendMessageAsync(message.ToDefaultJson()).NoContext()).Value.MessageId);
        }

        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
        {
            var client = context ?? await GetContextAsync();
            return !string.IsNullOrWhiteSpace((await client.SendMessageAsync(messages.ToDefaultJson()).NoContext()).Value.MessageId);
        }

        public async Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization)
        {
            var client = context ?? await GetContextAsync();
            _ = await client.SendMessageAsync(message.ToDefaultJson(), new TimeSpan(0, 0, delayInSeconds)).NoContext();
            return 0;
        }

        public Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
            => throw new NotImplementedException("Queue storage doesn't allow this operation.");
    }
}