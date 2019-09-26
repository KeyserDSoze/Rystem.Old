using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueIntegration
    {
        Task<bool> SendAsync(IQueue message);
        Task<long> SendScheduledAsync(IQueue message, int delayInSeconds);
        Task<bool> DeleteScheduledAsync(long messageId);
        Task<bool> SendBatchAsync(IEnumerable<IQueue> messages);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds);
    }
}
