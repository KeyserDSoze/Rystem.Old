using Microsoft.Azure.EventHubs;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
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
        public static void Install<TEntity>(string connectionString = null, string entityPath = null, Installation installation = Installation.Null)
            where TEntity : IEventHub, new()
        {
            Type type = typeof(TEntity);
            string key = installation == Installation.Null ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!eventHubs.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!eventHubs.ContainsKey(key))
                    {
                        Installer(type, key, connectionString, entityPath);
                    }
                }
            }
        }
        private static void Installer(Type type, string key, string connectionString = null, string entityPath = null)
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
            eventHubs.Add(key, EventHubClient.CreateFromConnectionString(
                new EventHubsConnectionStringBuilder(connectionStringWithEntityPath).ToString()));
        }
        private static EventHubClient Instance(Type type, Installation installation = Installation.Null)
        {
            string key = installation == Installation.Null ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!eventHubs.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!eventHubs.ContainsKey(key))
                    {
                        Activator.CreateInstance(type);
                        if (!eventHubs.ContainsKey(key))
                        {
                            Installer(type, key, null, null);
                        }
                    }
                }
            }
            return eventHubs[key];
        }
        public static async Task<bool> Send(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Null, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            EventHubMessage connectionMessage = new EventHubMessage()
            {
                Attempt = attempt,
                Container = eventHubEntity,
                Flow = flowType,
                Version = version,
                Installation = installation
            };
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(connectionMessage.ToJson()));
            await Instance(eventHubEntity.GetType(), installation).SendAsync(eventData);
            return true;
        }
        public static string GetEventHubName(this IEventHub eventHubEntity, Installation installation = Installation.Null)
        {
            return Instance(eventHubEntity.GetType(), installation).EventHubName;
        }
        public static async Task<DebugMessage> DebugSend(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Null, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            await Task.Delay(0);
            if (Instance(eventHubEntity.GetType(), installation) == null)
                throw new NotImplementedException("Please insert a correct connection string and entity path");
            else
                return new DebugMessage()
                {
                    EventData = new EventData(Encoding.UTF8.GetBytes(new EventHubMessage()
                    {
                        Attempt = attempt,
                        Container = eventHubEntity,
                        Flow = flowType,
                        Version = version,
                        Installation = installation
                    }.ToJson())),
                };
        }
    }
}
