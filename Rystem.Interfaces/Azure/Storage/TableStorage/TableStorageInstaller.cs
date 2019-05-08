using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rystem.Azure.Storage
{
    public static class TableStorageInstaller
    {
        private static string ConnectionStringDefault;
        private static Dictionary<string, Dictionary<Installation, (string connectionString, List<string> tableNames)>> Contexts = new Dictionary<string, Dictionary<Installation, (string connectionString, List<string> tableNames)>>();
        public static void ConfigureAsDefault(string connectionString)
        {
            ConnectionStringDefault = connectionString;
        }
        public static void Configure<Entity>(string connectionString, Installation installation = Installation.Default, params string[] tableNames) where Entity : new()
        {
            Type type = typeof(Entity);
            if (!Contexts.ContainsKey(type.FullName))
            {
                List<string> names = tableNames?.ToList();
                if (names.Count == 0) names.Add(type.Name);
                Contexts.Add(type.FullName, new Dictionary<Installation, (string connectionString, List<string> tableNames)>());
                Contexts[type.FullName].Add(installation, (connectionString, names));
            }
        }
        public static (string connectionString, List<string> tableNames) GetConnectionStringAndTableNames(Type type, Installation installation = Installation.Default)
        {
            try
            {
                if (Contexts.ContainsKey(type.FullName))
                    return Contexts[type.FullName][installation];
                if (!string.IsNullOrWhiteSpace(ConnectionStringDefault))
                    return (ConnectionStringDefault, new List<string>() { type.Name });
                throw new NotImplementedException("Please use Install static method in static constructor of your class to set ConnectionString and names of table.");
            }
            catch (Exception er)
            {
                throw new NotImplementedException($"Installation failed: {er}");
            }
        }
    }
}
