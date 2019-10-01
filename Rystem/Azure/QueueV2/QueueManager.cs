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
        where TEntity : IQueue
    {
        private readonly IQueueIntegration Integration;
        private readonly QueueConfiguration QueueConfiguration;
        public QueueManager()
        {
            QueueConfiguration = QueueInstaller.GetConfiguration<TEntity>();
            switch (QueueConfiguration.Type)
            {
                case QueueType.QueueStorage:
                    Integration = new QueueStorageIntegration(QueueConfiguration);
                    break;
                case QueueType.EventHub:
                    Integration = new EventHubIntegration(QueueConfiguration);
                    break;
                case QueueType.ServiceBus:
                    Integration = new ServiceBusIntegration(QueueConfiguration);
                    break;
                default:
                    throw new InvalidOperationException($"Wrong type installed {QueueConfiguration.Type}");
            }
        }
        public async Task<bool> SendAsync(IQueue message)
            => await Integration.SendAsync(message);
        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds)
            => await Integration.SendScheduledAsync(message, delayInSeconds);
        public async Task<bool> DeleteScheduledAsync(long messageId)
            => await Integration.DeleteScheduledAsync(messageId);
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages)
            => await Integration.SendBatchAsync(messages);
        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds)
            => await Integration.SendScheduledBatchAsync(messages, delayInSeconds);
        public async Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds = 0)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds = 0)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.ToJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() };
        }
        public string GetName() => QueueConfiguration.Name;
    }
}
