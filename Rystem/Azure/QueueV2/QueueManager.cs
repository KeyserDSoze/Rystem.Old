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
        private readonly IDictionary<Installation, IQueueIntegration<TEntity>> Integrations = new Dictionary<Installation, IQueueIntegration<TEntity>>();
        private readonly IDictionary<Installation, QueueConfiguration> QueueConfiguration;
        public QueueManager()
        {
            QueueConfiguration = QueueInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, QueueConfiguration> configuration in QueueConfiguration)
                switch (configuration.Value.Type)
                {
                    case QueueType.QueueStorage:
                        Integrations.Add(configuration.Key, new QueueStorageIntegration<TEntity>(configuration.Value));
                        break;
                    case QueueType.EventHub:
                        Integrations.Add(configuration.Key, new EventHubIntegration<TEntity>(configuration.Value));
                        break;
                    case QueueType.ServiceBus:
                        Integrations.Add(configuration.Key, new ServiceBusIntegration<TEntity>(configuration.Value));
                        break;
                    case QueueType.SmartQueue:
                        Integrations.Add(configuration.Key, new SmartQueueIntegration<TEntity>(configuration.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> SendAsync(IQueue message, Installation installation, int path, int organization)
            => await Integrations[installation].SendAsync(message, path, organization);
        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledAsync(message, delayInSeconds, path, organization);
        public async Task<bool> DeleteScheduledAsync(long messageId, Installation installation)
            => await Integrations[installation].DeleteScheduledAsync(messageId);
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, Installation installation, int path, int organization)
            => await Integrations[installation].SendBatchAsync(messages, path, organization);
        public async Task<IList<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledBatchAsync(messages, delayInSeconds, path, organization);
        public async Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToJson(), SmartMessage = message.ToJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0);
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.ToJson(), SmartMessage = messages.ToJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() };
        }
        public string GetName(Installation installation) => QueueConfiguration[installation].Name;

        public async Task<IEnumerable<TQueue>> ReadAsync<TQueue>(Installation installation, int path, int organization)
            where TQueue : IQueue
            => (await Integrations[installation].Read(path, organization)).Select(x => (TQueue)(x as IQueue));
    }
}
