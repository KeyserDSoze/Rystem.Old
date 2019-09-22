using Rystem.Enums;
using Rystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Azure.Queue
{
    public static class QueueInstaller
    {
        public static void ConfigureAsDefault(QueueConfiguration configuration)
            => Installer<QueueConfiguration>.ConfigureAsDefault(configuration);
        public static void Configure<Entity>(QueueConfiguration configuration)
            where Entity : IQueueMessage
            => Installer<QueueConfiguration, Entity>.Configure(configuration, Installation.Default);
        public static QueueConfiguration GetConfiguration<Entity>()
            where Entity : IQueueMessage
            => Installer<QueueConfiguration, Entity>.GetConfiguration(Installation.Default);
    }
    public class QueueConfiguration : IRystemConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public QueueType Type { get; set; }
    }
    public enum QueueType
    {
        QueueStorage,
        EventHub,
        ServiceBus
    }
}
