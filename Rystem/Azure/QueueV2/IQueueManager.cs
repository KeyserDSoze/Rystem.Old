using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueManager
    {
        Task<bool> SendAsync(IQueueMessage message);
        Task<long> SendScheduledAsync(IQueueMessage message, int delayInSeconds);
        Task<bool> DeleteScheduledAsync(long messageId);
        Task<bool> SendBatchAsync(IEnumerable<IQueueMessage> messages);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds);
        Task<DebugMessage> DebugSendAsync(IQueueMessage message, int delayInSeconds = 0);
        Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds = 0);
    }
}
