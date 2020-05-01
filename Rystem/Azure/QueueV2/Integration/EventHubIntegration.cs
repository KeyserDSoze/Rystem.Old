﻿using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class EventHubIntegration<TEntity> : IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        private readonly EventHubClient Client;
        internal EventHubIntegration(QueueConfiguration property) 
            => this.Client = EventHubClient.CreateFromConnectionString(
                  new EventHubsConnectionStringBuilder($"{property.ConnectionString};EntityPath={property.Name}").ToString());

        public Task<bool> CleanAsync() => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<bool> DeleteScheduledAsync(long messageId) => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<IEnumerable<TEntity>> Read(int path, int organization) => throw new NotImplementedException();

        public async Task<bool> SendAsync(IQueue message, int path, int organization)
        {
            await this.Client.SendAsync(new EventData(message.ToSendable()));
            return true;
        }
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization)
        {
            EventDataBatch eventDataBatch = this.Client.CreateBatch();
            foreach (IQueue message in messages)
                eventDataBatch.TryAdd(new EventData(message.ToSendable()));
            await this.Client.SendAsync(eventDataBatch);
            return true;
        }
        public Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization) => throw new NotImplementedException("Event hub doesn't allow this operation.");
        public Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization) => throw new NotImplementedException("Event hub doesn't allow this operation.");
    }
}
