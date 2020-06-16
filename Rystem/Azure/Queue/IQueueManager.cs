using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.Queue
{
    internal interface IQueueManager<TQueue> : IManager<TQueue>
    {
        Task<bool> SendAsync(TQueue message, Installation installation, int path, int organization);
        Task<long> SendScheduledAsync(TQueue message, int delayInSeconds, Installation installation, int path, int organization);
        Task<bool> DeleteScheduledAsync(long messageId, Installation installation);
        Task<bool> SendBatchAsync(IEnumerable<TQueue> messages, Installation installation, int path, int organization);
        Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TQueue> messages, int delayInSeconds, Installation installation, int path, int organization);
        Task<IEnumerable<TQueue>> ReadAsync(Installation installation, int path, int organization);
        Task<bool> CleanAsync(Installation installation);
        Task<DebugMessage> DebugSendAsync(TQueue message, int delayInSeconds, Installation installation, int path, int organization);
        Task<DebugMessage> DebugSendBatchAsync(IEnumerable<TQueue> messages, int delayInSeconds, Installation installation, int path, int organization);
        string GetName(Installation installation);
    }
}
