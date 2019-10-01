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
        private readonly IDictionary<Installation, IQueueIntegration> Integrations;
        private readonly IDictionary<Installation, QueueConfiguration> QueueConfiguration;
        public QueueManager()
        {
            Integrations = new Dictionary<Installation, IQueueIntegration>();
            QueueConfiguration = QueueInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, QueueConfiguration> configuration in QueueConfiguration)
                switch (configuration.Value.Type)
                {
                    case QueueType.QueueStorage:
                        Integrations.Add(configuration.Key, new QueueStorageIntegration(configuration.Value));
                        break;
                    case QueueType.EventHub:
                        Integrations.Add(configuration.Key, new EventHubIntegration(configuration.Value));
                        break;
                    case QueueType.ServiceBus:
                        Integrations.Add(configuration.Key, new ServiceBusIntegration(configuration.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> SendAsync(IQueue message, Installation installation = Installation.Default)
            => await Integrations[installation].SendAsync(message);
        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, Installation installation = Installation.Default)
            => await Integrations[installation].SendScheduledAsync(message, delayInSeconds);
        public async Task<bool> DeleteScheduledAsync(long messageId, Installation installation = Installation.Default)
            => await Integrations[installation].DeleteScheduledAsync(messageId);
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, Installation installation = Installation.Default)
            => await Integrations[installation].SendBatchAsync(messages);
        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation = Installation.Default)
            => await Integrations[installation].SendScheduledBatchAsync(messages, delayInSeconds);
        public async Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds = 0, Installation installation = Installation.Default)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds = 0, Installation installation = Installation.Default)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.ToJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() };
        }
        public string GetName(Installation installation = Installation.Default) => QueueConfiguration[installation].Name;
    }
}
