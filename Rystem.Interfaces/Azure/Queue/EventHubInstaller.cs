using Rystem.Enums;
using Rystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Rystem.Interfaces.Installer;

namespace Rystem.Azure.Queue
{
    public static class EventHubInstaller
    {
        private static Type InstallerType = typeof(IEventHub);
        public static void ConfigureAsDefault(string connectionString, string entityPath)
        {
            InstallerType.ConfigureAsDefault(connectionString, entityPath);
        }
        public static void Configure<Entity>(string connectionString, string entityPath = null, Installation installation = Installation.Default)
            where Entity : IEventHub
        {
            InstallerType.Configure(typeof(Entity), connectionString, installation, entityPath);
        }
        public static string GetCompleteConnectionStringAndEntityPath(Type type, Installation installation = Installation.Default)
        {
            InternalConnection internalConnection = InstallerType.GetConfiguration(type, installation);
            return $"{internalConnection.ConnectionString};EntityPath={internalConnection.Names.FirstOrDefault().ToLower()}";
        }
    }
}
