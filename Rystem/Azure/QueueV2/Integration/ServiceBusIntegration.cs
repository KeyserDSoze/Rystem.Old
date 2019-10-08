using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Rystem.Interfaces.Const;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class ServiceBusIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        private QueueClient QueueClient;
        internal ServiceBusIntegration(QueueConfiguration property) =>
            this.QueueClient = new QueueClient(
                new ServiceBusConnectionStringBuilder(
                    $"{property.ConnectionString};EntityPath={property.Name}"),
                    ReceiveMode.PeekLock);
        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            await this.QueueClient.CancelScheduledMessageAsync(messageId);
            return true;
        }

        public Task<IList<TEntity>> Read(int path, int organization)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendAsync(IQueue message, int path, int organization)
        {
            await this.QueueClient.SendAsync(
                new Message(message.ToSendable()));
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization)
        {
            await this.QueueClient.SendAsync(
                new Message(messages.ToSendable()));
            return true;
        }

        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization)
        {
            return await this.QueueClient.ScheduleMessageAsync(
                 new Message(message.ToSendable()), DateTime.UtcNow.AddSeconds(delayInSeconds));
        }

        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization)
        {
            return new List<long>()
            {
                await this.QueueClient.ScheduleMessageAsync(
                    new Message(messages.ToSendable()), DateTime.UtcNow.AddSeconds(delayInSeconds))
            };
        }
    }
}
