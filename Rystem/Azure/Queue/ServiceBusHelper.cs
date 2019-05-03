using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public interface IServiceBus { }
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
        public static void Install<TEntity>(string connectionString = null, string entityPath = null) where TEntity : IServiceBus, new()
        {
            Type type = typeof(TEntity);
            if (!seriviceBuses.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!seriviceBuses.ContainsKey(type.FullName))
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
            seriviceBuses.Add(type.FullName, new QueueClient(new ServiceBusConnectionStringBuilder(connectionStringWithEntityPath), ReceiveMode.PeekLock));

        }
        private static QueueClient Instance(Type type)
        {
            if (!seriviceBuses.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!seriviceBuses.ContainsKey(type.FullName))
                    {
                        Activator.CreateInstance(type);
                        if (!seriviceBuses.ContainsKey(type.FullName))
                        {
                            Installer(type, null, null);
                        }
                    }
                }
            }
            return seriviceBuses[type.FullName];
        }
        public static async Task<long> Send(this IServiceBus serviceBusEntity, int delayInSeconds = 0, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(new EventHubMessage()
            {
                Attempt = attempt,
                Container = serviceBusEntity,
                Flow = flowType,
                Version = version,
            }.ToJson()));
            if (delayInSeconds == 0)
                await Instance(serviceBusEntity.GetType()).SendAsync(message);
            else
                return await Instance(serviceBusEntity.GetType()).ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delayInSeconds));
            return 0;
        }
        public static async Task<bool> Delete(this IServiceBus serviceBusEntity, long messageId)
        {
            //It's not possible to delete an active message, it's possible to delete only scheduled messages
            try
            {
                if (messageId > 0)
                    await Instance(serviceBusEntity.GetType()).CancelScheduledMessageAsync(messageId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
