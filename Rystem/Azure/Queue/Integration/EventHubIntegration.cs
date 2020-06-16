using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Queue
{
    internal class EventHubIntegration<TEntity> : IQueueIntegration<TEntity>
    {
        private readonly EventHubClient Client;
        internal EventHubIntegration(QueueConfiguration property) 
            => this.Client = EventHubClient.CreateFromConnectionString(
                  new EventHubsConnectionStringBuilder($"{property.ConnectionString};EntityPath={property.Name}").ToString());

        public Task<bool> CleanAsync() 
            => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<bool> DeleteScheduledAsync(long messageId) 
            => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<IEnumerable<TEntity>> ReadAsync(int path, int organization) 
            => throw new NotImplementedException("Event hub doesn't allow this operation.");

        public async Task<bool> SendAsync(TEntity message, int path, int organization)
        {
            await this.Client.SendAsync(new EventData(message.ToSendable())).NoContext();
            return true;
        }
        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, int path, int organization)
        {
            EventDataBatch eventDataBatch = this.Client.CreateBatch();
            foreach (TEntity message in messages)
                eventDataBatch.TryAdd(new EventData(message.ToSendable()));
            await this.Client.SendAsync(eventDataBatch).NoContext();
            return true;
        }
        public Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, int path, int organization) 
            => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, int path, int organization) 
            => throw new NotImplementedException("Event hub doesn't allow this operation.");
    }
}
