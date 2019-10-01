using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueManager
    {
        Task<bool> SendAsync(IQueue message, Installation installation = Installation.Default);
        Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, Installation installation = Installation.Default);
        Task<bool> DeleteScheduledAsync(long messageId, Installation installation = Installation.Default);
        Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, Installation installation = Installation.Default);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation = Installation.Default);
        Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds = 0, Installation installation = Installation.Default);
        Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds = 0, Installation installation = Installation.Default);
        string GetName(Installation installation = Installation.Default);
    }
}
