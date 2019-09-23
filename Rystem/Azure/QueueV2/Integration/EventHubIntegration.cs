using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class EventHubIntegration : IQueueIntegration
    {
        private EventHubClient Client;
        internal EventHubIntegration(QueueConfiguration property)
        {
            this.Client = EventHubClient.CreateFromConnectionString(
                     new EventHubsConnectionStringBuilder($"{property.ConnectionString};EntityPath={property.Name}").ToString());
        }
        public async Task<bool> DeleteScheduledAsync(long messageId)
        {
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }

        public async Task<bool> SendAsync(IQueueMessage message)
        {
            await this.Client.SendAsync(new EventData(message.ToSendable()));
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<IQueueMessage> messages)
        {
            EventDataBatch eventDataBatch = this.Client.CreateBatch();
            foreach (IQueueMessage message in messages)
                eventDataBatch.TryAdd(new EventData(message.ToSendable()));
            await this.Client.SendAsync(eventDataBatch);
            return true;
        }

        public async Task<long> SendScheduledAsync(IQueueMessage message, int delayInSeconds)
        {
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }

        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds)
        {
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }
    }
}
