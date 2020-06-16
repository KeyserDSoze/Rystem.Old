using Microsoft.Azure.EventHubs;
using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Queue
{
    internal class QueueManager<TEntity> : IQueueManager<TEntity>
    {
        private readonly IDictionary<Installation, IQueueIntegration<TEntity>> Integrations = new Dictionary<Installation, IQueueIntegration<TEntity>>();
        private readonly IDictionary<Installation, QueueConfiguration> QueueConfiguration;
        private static readonly object TrafficLight = new object();
        private IQueueIntegration<TEntity> Integration(Installation installation)
        {
            if (!Integrations.ContainsKey(installation))
                lock (TrafficLight)
                    if (!Integrations.ContainsKey(installation))
                    {
                        QueueConfiguration configuration = QueueConfiguration[installation];
                        switch (configuration.Type)
                        {
                            case QueueType.QueueStorage:
                                Integrations.Add(installation, new QueueStorageIntegration<TEntity>(configuration));
                                break;
                            case QueueType.EventHub:
                                Integrations.Add(installation, new EventHubIntegration<TEntity>(configuration));
                                break;
                            case QueueType.ServiceBus:
                                Integrations.Add(installation, new ServiceBusIntegration<TEntity>(configuration));
                                break;
                            case QueueType.SmartQueue:
                                Integrations.Add(installation, new SmartQueueIntegration<TEntity>(configuration));
                                break;
                            default:
                                throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
                        }
                    }
            return Integrations[installation];
        }
        public InstallerType InstallerType => InstallerType.Queue;
        private readonly TEntity DefaultEntity;
        public QueueManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            this.DefaultEntity = entity;
            QueueConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as QueueConfiguration);
        }
        public async Task<bool> SendAsync(TEntity message, Installation installation, int path, int organization)
            => await Integration(installation).SendAsync(message, path, organization).NoContext();
        public async Task<long> SendScheduledAsync(TEntity message, int delayInSeconds, Installation installation, int path, int organization)
            => await Integration(installation).SendScheduledAsync(message, delayInSeconds, path, organization).NoContext();
        public async Task<bool> DeleteScheduledAsync(long messageId, Installation installation)
            => await Integration(installation).DeleteScheduledAsync(messageId).NoContext();
        public async Task<bool> SendBatchAsync(IEnumerable<TEntity> messages, Installation installation, int path, int organization)
            => await Integration(installation).SendBatchAsync(messages.Select(x => x), path, organization).NoContext();
        public async Task<IEnumerable<long>> SendScheduledBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, Installation installation, int path, int organization)
            => await Integration(installation).SendScheduledBatchAsync(messages.Select(x => x), delayInSeconds, path, organization).NoContext();
        public async Task<IEnumerable<TEntity>> ReadAsync(Installation installation, int path, int organization)
           => await Integration(installation).ReadAsync(path, organization).NoContext();
        public async Task<bool> CleanAsync(Installation installation)
            => await Integration(installation).CleanAsync().NoContext();
        public Task<DebugMessage> DebugSendAsync(TEntity message, int delayInSeconds, Installation installation, int path, int organization)
            => Task.FromResult(new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = message.ToDefaultJson(), SmartMessage = message.ToDefaultJson(), EventDatas = new EventData[1] { new EventData(message.ToSendable()) } });
        public Task<DebugMessage> DebugSendBatchAsync(IEnumerable<TEntity> messages, int delayInSeconds, Installation installation, int path, int organization)
            => Task.FromResult(new DebugMessage() { DelayInSeconds = delayInSeconds, ServiceBusMessage = messages.Select(x => x).ToDefaultJson(), SmartMessage = messages.Select(x => x).ToDefaultJson(), EventDatas = messages.Select(x => new EventData(x.ToSendable())).ToArray() });
        public string GetName(Installation installation) => QueueConfiguration[installation].Name;
    }
}