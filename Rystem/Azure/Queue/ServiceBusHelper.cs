using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public static class ServiceBusHelper
    {
        private static Dictionary<string, string> connectionStrings = new Dictionary<string, string>();
        private static Dictionary<string, QueueClient> seriviceBuses = new Dictionary<string, QueueClient>();
        private static readonly object TrafficLight = new object();
        private static string connectionStringDefault;
        private static string entityPathDefault;
        public static void Install(string connectionString, string entityPath = null)
        {
            connectionStringDefault = connectionString;
            entityPathDefault = entityPath;
        }
        public static void Install<TEntity>(string connectionString = null, string entityPath = null, Installation installation = Installation.Null) where TEntity : IServiceBus, new()
        {
            Type type = typeof(TEntity);
            string key = installation == Installation.Null ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!seriviceBuses.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!seriviceBuses.ContainsKey(key))
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
            seriviceBuses.Add(key, new QueueClient(new ServiceBusConnectionStringBuilder(connectionStringWithEntityPath), ReceiveMode.PeekLock));

        }
        private static QueueClient Instance(Type type, Installation installation = Installation.Null)
        {
            string key = installation == Installation.Null ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!seriviceBuses.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!seriviceBuses.ContainsKey(key))
                    {
                        Activator.CreateInstance(type);
                        if (!seriviceBuses.ContainsKey(key))
                        {
                            Installer(type, key, null, null);
                        }
                    }
                }
            }
            return seriviceBuses[key];
        }
        public static async Task<long> Send(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Null, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(new ServiceBusMessage()
            {
                Attempt = attempt,
                Container = serviceBusEntity,
                Flow = flowType,
                Version = version,
                Installation = installation
            }.ToJson()));
            if (delayInSeconds == 0)
                await Instance(serviceBusEntity.GetType(), installation).SendAsync(message);
            else
                return await Instance(serviceBusEntity.GetType(), installation).ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delayInSeconds));
            return 0;
        }
        public static string GetServiceBusName(this IServiceBus serviceBusEntity, Installation installation = Installation.Null)
        {
            return Instance(serviceBusEntity.GetType(), installation).QueueName;
        }
        public static async Task<DebugMessage> DebugSend(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Null, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            await Task.Delay(0);
            if (Instance(serviceBusEntity.GetType(), installation) == null)
                throw new NotImplementedException("Please insert a correct connection string and entity path");
            else
                return new DebugMessage()
                {
                    DelayInSeconds = delayInSeconds,
                    Message = new Message(Encoding.UTF8.GetBytes(new ServiceBusMessage()
                    {
                        Attempt = attempt,
                        Container = serviceBusEntity,
                        Flow = flowType,
                        Version = version,
                        Installation = installation
                    }.ToJson()))
                };
        }
        public static async Task<bool> Delete(this IServiceBus serviceBusEntity, long messageId, Installation installation = Installation.Null)
        {
            //It's not possible to delete an active message, it's possible to delete only scheduled messages
            try
            {
                if (messageId > 0)
                    await Instance(serviceBusEntity.GetType(), installation).CancelScheduledMessageAsync(messageId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
