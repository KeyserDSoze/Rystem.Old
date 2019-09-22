using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public interface IQueueIntegration
    {
        Task<bool> SendAsync(IQueueMessage message);
        Task<long> SendScheduledAsync(IQueueMessage message, int delayInSeconds);
        Task<bool> DeleteScheduledAsync(long messageId);
        Task<bool> SendBatchAsync(IEnumerable<IQueueMessage> messages);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds);
    }
}
