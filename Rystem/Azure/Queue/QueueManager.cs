using Microsoft.Azure.EventHubs;
using Rystem.Debug;
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
            => await Integrations[installation].SendAsync((TEntity)message, path, organization).NoContext();
        public async Task<long> SendScheduledAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledAsync((TEntity)message, delayInSeconds, path, organization).NoContext();
        public async Task<bool> DeleteScheduledAsync(long messageId, Installation installation)
            => await Integrations[installation].DeleteScheduledAsync(messageId).NoContext();
        public async Task<bool> SendBatchAsync(IEnumerable<IQueue> messages, Installation installation, int path, int organization)
            => await Integrations[installation].SendBatchAsync(messages.Select(x => (TEntity)x), path, organization).NoContext();
        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledBatchAsync(messages.Select(x => (TEntity)x), delayInSeconds, path, organization).NoContext();
        public async Task<DebugMessage> DebugSendAsync(IQueue message, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0).NoContext();
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = ((TEntity)message).ToDefaultJson(), SmartMessage = ((TEntity)message).ToDefaultJson(), EventDatas = new EventData[1] { new EventData(((TEntity)message).ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<IQueue> messages, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0).NoContext();
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.Select(x => (TEntity)x).ToDefaultJson(), SmartMessage = messages.Select(x => (TEntity)x).ToDefaultJson(), EventDatas = messages.Select(x => new EventData(((TEntity)x).ToSendable())).ToArray() };
        }
        public string GetName(Installation installation) => QueueConfiguration[installation].Name;

        public async Task<IEnumerable<TQueue>> ReadAsync<TQueue>(Installation installation, int path, int organization)
            where TQueue : IQueue
            => (await Integrations[installation].ReadAsync(path, organization).NoContext()).Select(x => (TQueue)(x as IQueue));

        public async Task<bool> CleanAsync(Installation installation)
            => await Integrations[installation].CleanAsync().NoContext();
    }
}
