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
        public static void ConfigureAsDefault(QueueProperty property)
            => Installer<QueueProperty>.ConfigureAsDefault(property);
        public static void Configure<Entity>(QueueProperty property)
            where Entity : IQueueMessage
            => Installer<QueueProperty, Entity>.Configure(property, Installation.Default);
        public static QueueProperty GetConfiguration<Entity>()
            where Entity : IQueueMessage
            => Installer<QueueProperty, Entity>.GetConfiguration(Installation.Default);
    }
    public class QueueProperty
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
