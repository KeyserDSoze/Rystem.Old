using Rystem.Enums;
using Rystem.Interfaces;
using System;
using System.Collections.Generic;
using static Rystem.Interfaces.Installer;

namespace Rystem.Azure.Storage
{
    public static class TableStorageInstaller
    {
        private static Type InstallerType = typeof(ITableStorage);
        public static void ConfigureAsDefault(string connectionString)
        {
            InstallerType.ConfigureAsDefault(connectionString);
        }
        public static void Configure<Entity>(string connectionString, Installation installation = Installation.Default, params string[] tableNames)
            where Entity : ITableStorage
        {
            InstallerType.Configure(typeof(Entity), connectionString, installation, tableNames);
        }
        public static (string connectionString, List<string> tableNames) GetConnectionStringAndTableNames(Type type, Installation installation = Installation.Default)
        {
            InternalConnection internalConnection = InstallerType.GetConfiguration(type, installation);
            return (internalConnection.ConnectionString, internalConnection.Names);
        }
    }
}
