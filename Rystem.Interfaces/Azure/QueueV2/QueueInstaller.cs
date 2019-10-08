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
        public static void Configure<Entity>(QueueConfiguration configuration, Installation installation = Installation.Default)
            where Entity : IQueue
            => Installer<QueueConfiguration, Entity>.Configure(configuration, installation);
        public static IDictionary<Installation, QueueConfiguration> GetConfiguration<Entity>()
            where Entity : IQueue
            => Installer<QueueConfiguration, Entity>.GetConfiguration();
    }
    public class QueueConfiguration : IRystemConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public QueueType Type { get; set; }
    }
    public enum QueueType
    {
        EventHub,
        QueueStorage,
        ServiceBus,
        SmartQueue
    }
}
