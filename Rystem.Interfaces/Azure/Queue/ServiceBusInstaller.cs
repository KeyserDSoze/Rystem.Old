using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Azure.Queue
{
    public static class ServiceBusInstaller
    {
        private static string ConnectionStringDefault;
        private static string EntityPathDefault;
        private static Dictionary<string, Dictionary<Installation, (string connectionString, string entityPath)>> Contexts = new Dictionary<string, Dictionary<Installation, (string connectionString, string entityPath)>>();
        public static void ConfigureAsDefault(string connectionString, string entityPath = null)
        {
            ConnectionStringDefault = connectionString;
            EntityPathDefault = entityPath;
        }
        public static void Configure<Entity>(string connectionString, string entityPath = null, Installation installation = Installation.Default) where Entity : new()
        {
            Type type = typeof(Entity);
            if (!Contexts.ContainsKey(type.FullName))
                Contexts.Add(type.FullName, new Dictionary<Installation, (string connectionString, string entityPath)>());
            if (!Contexts[type.FullName].ContainsKey(installation))
                Contexts[type.FullName].Add(installation, (connectionString, entityPath));
        }
        public static string GetCompleteConnectionStringAndEntityPath(Type type, Installation installation = Installation.Default)
        {
            if (Contexts.ContainsKey(type.FullName))
                if(Contexts[type.FullName].ContainsKey(installation))
                return $"{Contexts[type.FullName][installation].connectionString};EntityPath={(string.IsNullOrWhiteSpace(Contexts[type.FullName][installation].entityPath) ? type.Name.ToLower() : Contexts[type.FullName][installation].entityPath.ToLower())}";
            else
                    throw new NotImplementedException("Please use right installation.");
            if (!string.IsNullOrWhiteSpace(ConnectionStringDefault))
                return $"{ConnectionStringDefault};EntityPath={(string.IsNullOrWhiteSpace(EntityPathDefault) ? type.Name.ToLower() : EntityPathDefault.ToLower())}";
            throw new NotImplementedException("Please use Install static method in static constructor of your class to set ConnectionString and Entity Path.");
        }
    }
}
