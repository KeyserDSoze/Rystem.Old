using System;
using System.Collections.Generic;

namespace Rystem.Azure.Queue
{
    public static class QueueInstaller
    {
        public static void ConfigureAsDefault(QueueConfiguration configuration)
            => Installer<QueueConfiguration>.ConfigureAsDefault(configuration);
        public static void Configure<Entity>(QueueConfiguration configuration, Installation installation = Installation.Default)
            where Entity : IQueue, new()
            => Installer<QueueConfiguration, Entity>.Configure(configuration, installation);
        public static IDictionary<Installation, QueueConfiguration> GetConfiguration<Entity>()
            where Entity : IQueue, new()
            => Installer<QueueConfiguration, Entity>.GetConfiguration();
    }
}
