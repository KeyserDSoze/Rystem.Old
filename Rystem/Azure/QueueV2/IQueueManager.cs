using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueManager
    {
        Task<bool> SendAsync(IQueue message);
        Task<long> SendScheduledAsync(IQueue message, int delayInSeconds);
        Task<bool> DeleteScheduledAsync(long messageId);
        Task<bool> SendBatchAsync(IEnumerable<IQueue> messages);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds);
        Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds = 0);
        Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds = 0);
        string GetName();
    }
}
