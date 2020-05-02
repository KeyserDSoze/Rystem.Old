using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueManager
    {
        Task<bool> SendAsync(IQueue message, Installation installation, int path, int organization);
        Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization);
        Task<bool> DeleteScheduledAsync(long messageId, Installation installation);
        Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, Installation installation, int path, int organization);
        Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization);
        Task<IEnumerable<TEntity>> ReadAsync<TEntity>(Installation installation, int path, int organization)
            where TEntity : IQueue;
        Task<bool> CleanAsync(Installation installation);
        Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization);
        Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization);
        string GetName(Installation installation);
    }
}
