using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class ServiceBusIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue, new()
    {
        private readonly QueueClient QueueClient;
        internal ServiceBusIntegration(QueueConfiguration property) =>
            this.QueueClient = new QueueClient(
                new ServiceBusConnectionStringBuilder(
                    $"{property.ConnectionString};EntityPath={property.Name}"),
                    ReceiveMode.PeekLock);

        public Task<bool> CleanAsync()
            => throw new NotImplementedException("ServiceBus doesn't allow this operation.");

        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            await this.QueueClient.CancelScheduledMessageAsync(messageId).NoContext();
            return true;
        }

        public Task<IEnumerable<TEntity>> ReadAsync(int path, int organization)
            => throw new NotImplementedException("ServiceBus doesn't allow this operation.");

        public async Task<bool> SendAsync(TEntity message, int path, int organization)
        {
            await this.QueueClient.SendAsync(new Message(message.ToSendable())).NoContext();
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
        {
            await this.QueueClient.SendAsync(new Message(messages.ToSendable())).NoContext();
            return true;
        }

        public async Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization)
            => await this.QueueClient.ScheduleMessageAsync(new Message(message.ToSendable()), DateTime.UtcNow.AddSeconds(delayInSeconds)).NoContext();

        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization)
            => new List<long>()
                {
                    await this.QueueClient.ScheduleMessageAsync(new Message(messages.ToSendable()), DateTime.UtcNow.AddSeconds(delayInSeconds)).NoContext()
                };
    }
}
