using Rystem.Enums;
using Rystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Rystem.Interfaces.Installer;

namespace Rystem.Azure.Queue
{
    public static class QueueStorageInstaller
    {
        private static Type InstallerType = typeof(IQueueStorage);
        public static void ConfigureAsDefault(string connectionString, string queueName)
        {
            InstallerType.ConfigureAsDefault(connectionString, queueName);
        }
        public static void Configure<Entity>(string connectionString, string queueName = null, Installation installation = Installation.Default)
            where Entity : IEventHub
        {
            InstallerType.Configure(typeof(Entity), connectionString, installation, queueName);
        }
        public static (string connectionString, string queueName) GetCompleteConnectionStringAndQueueNames(Type type, Installation installation = Installation.Default)
        {
            InternalConnection internalConnection = InstallerType.GetConfiguration(type, installation);
            return (internalConnection.ConnectionString, internalConnection.Names.FirstOrDefault());
        }
    }
}
