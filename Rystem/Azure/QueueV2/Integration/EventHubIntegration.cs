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
            await Task.Delay(0);
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }

        public async Task<bool> SendAsync(IQueue message)
        {
            await this.Client.SendAsync(new EventData(message.ToSendable()));
            return true;
        }

        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages)
        {
            EventDataBatch eventDataBatch = this.Client.CreateBatch();
            foreach (IQueue message in messages)
                eventDataBatch.TryAdd(new EventData(message.ToSendable()));
            await this.Client.SendAsync(eventDataBatch);
            return true;
        }

        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds)
        {
            await Task.Delay(0);
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }

        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds)
        {
            await Task.Delay(0);
            throw new NotImplementedException("Event hub doesn't allow this operation.");
        }
    }
}
