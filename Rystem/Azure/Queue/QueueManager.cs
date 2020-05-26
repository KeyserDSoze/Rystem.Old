using Microsoft.Azure.EventHubs;
using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    internal class QueueManager<TEntity> : IQueueManager<TEntity>
    {
        private readonly IDictionary<Installation, IQueueIntegration<TEntity>> Integrations = new Dictionary<Installation, IQueueIntegration<TEntity>>();
        private readonly IDictionary<Installation, QueueConfiguration> QueueConfiguration;
        private readonly Type EntityType;

        public InstallerType InstallerType => InstallerType.Queue;

        public QueueManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            this.EntityType = entity.GetType();
            QueueConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as QueueConfiguration);
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
        public async Task<bool> SendAsync(TEntity message, Installation installation, int path, int organization)
            => await Integrations[installation].SendAsync(message, path, organization).NoContext();
        public async Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledAsync(message, delayInSeconds, path, organization).NoContext();
        public async Task<bool> DeleteScheduledAsync(long messageId, Installation installation)
            => await Integrations[installation].DeleteScheduledAsync(messageId).NoContext();
        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, Installation installation, int path, int organization)
            => await Integrations[installation].SendBatchAsync(messages.Select(x => x), path, organization).NoContext();
        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, Installation installation, int path, int organization)
            => await Integrations[installation].SendScheduledBatchAsync(messages.Select(x => x), delayInSeconds, path, organization).NoContext();
        public async Task<DebugMessage> DebugSendAsync(TEntity message, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0).NoContext();
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToDefaultJson(), SmartMessage = message.ToDefaultJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } };
        }
        public async Task<DebugMessage> DebugSendBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, Installation installation, int path, int organization)
        {
            await Task.Delay(0).NoContext();
            return new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.Select(x => x).ToDefaultJson(), SmartMessage = messages.Select(x => x).ToDefaultJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() };
        }
        public string GetName(Installation installation) => QueueConfiguration[installation].Name;

        public async Task<IEnumerable<TEntity>> ReadAsync(Installation installation, int path, int organization)
            => await Integrations[installation].ReadAsync(path, organization).NoContext();

        public async Task<bool> CleanAsync(Installation installation)
            => await Integrations[installation].CleanAsync().NoContext();
    }
}
