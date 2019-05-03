using Microsoft.Azure.EventHubs;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public interface IEventHub { }
    public static class EventHubHelper
    {
        private static Dictionary<string, string> connectionStrings = new Dictionary<string, string>();
        private static Dictionary<string, EventHubClient> eventHubs = new Dictionary<string, EventHubClient>();
        private static readonly object TrafficLight = new object();
        private static string connectionStringDefault;
        private static string entityPathDefault;
        public static void Install(string connectionString, string entityPath = null)
        {
            connectionStringDefault = connectionString;
            entityPathDefault = entityPath;
        }
        public static void Install<TEntity>(string connectionString = null, string entityPath = null) where TEntity : IEventHub, new()
        {
            Type type = typeof(TEntity);
            if (!eventHubs.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!eventHubs.ContainsKey(type.FullName))
                    {
                        Installer(type, connectionString, entityPath);
                    }
                }
            }
        }
        private static void Installer(Type type, string connectionString = null, string entityPath = null)
        {
            string connectionStringWithEntityPath;
            if (connectionString != null)
                connectionStringWithEntityPath = connectionString;
            else
                connectionStringWithEntityPath = connectionStringDefault;
            if (entityPath != null)
                connectionStringWithEntityPath += $";EntityPath={entityPath}";
            else if (entityPathDefault != null)
                connectionStringWithEntityPath += $";EntityPath={entityPathDefault}";
            else
                connectionStringWithEntityPath += $";EntityPath={type.Name.ToLower()}";
            eventHubs.Add(type.FullName, EventHubClient.CreateFromConnectionString(
                new EventHubsConnectionStringBuilder(connectionStringWithEntityPath).ToString()));
        }
        private static EventHubClient Instance(Type type)
        {
            if (!eventHubs.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!eventHubs.ContainsKey(type.FullName))
                    {
                        Activator.CreateInstance(type);
                        if (!eventHubs.ContainsKey(type.FullName))
                        {
                            Installer(type, null, null);
                        }
                    }
                }
            }
            return eventHubs[type.FullName];
        }
        public static async Task<bool> Send(this IEventHub eventHubEntity, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            ConnectionMessage connectionMessage = new ConnectionMessage()
            {
                Attempt = attempt,
                Container = eventHubEntity,
                Flow = flowType,
                Version = version
            };
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(connectionMessage.ToJson()));
            await Instance(eventHubEntity.GetType()).SendAsync(eventData);
            return true;
        }
    }
}
