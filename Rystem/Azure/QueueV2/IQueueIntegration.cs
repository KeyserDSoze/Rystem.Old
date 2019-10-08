using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal interface IQueueIntegration<TEntity>
        where TEntity : IQueue
    {
        Task<bool> SendAsync(IQueue message, int path, int organization);
        Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, int path, int organization);
        Task<bool> DeleteScheduledAsync(long messageId);
        Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, int path, int organization);
        Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, int path, int organization);
        Task<IList<TEntity>> Read(int path, int organization);
    }
}
