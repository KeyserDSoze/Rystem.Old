using Microsoft.Azure.EventHubs;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class QueueManager<TEntity> : IQueueManager
        where TEntity : IQueueMessage
    {
        private readonly IQueueIntegration Integration;
        public QueueManager()
        {
            QueueConfiguration configuration = QueueInstaller.GetConfiguration<TEntity>();
            switch (configuration.Type)
            {
                case QueueType.QueueStorage:
                    Integration = new QueueStorageIntegration(configuration);
                    break;
                case QueueType.EventHub:
                    Integration = new EventHubIntegration(configuration);
                    break;
                case QueueType.ServiceBus:
                    Integration = new ServiceBusIntegration(configuration);
                    break;
                default:
                    throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
            }
        }
        public async Task<bool> SendAsync(IQueueMessage message)
            => await Integration.SendAsync(message);
        public async Task<long> SendScheduledAsync(IQueueMessage message, int delayInSeconds)
            => await Integration.SendScheduledAsync(message, delayInSeconds);
        public async Task<bool> DeleteScheduledAsync(long messageId)
            => await Integration.DeleteScheduledAsync(messageId);
        public async Task<bool> SendBatchAsync(IEnumerable<IQueueMessage> messages)
            => await Integration.SendBatchAsync(messages);
        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds)
            => await Integration.SendScheduledBatchAsync(messages, delayInSeconds);
        public async Task<DebugMessage> DebugSendAsync(IQueueMessage message, int delayInSeconds = 0)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueueMessage> messages, int delayInSeconds = 0)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.ToJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() };
        }
    }
}
