using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public static class ServiceBusExtension
    {
        public static long Send(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return serviceBusEntity.SendAsync(delayInSeconds, installation, attempt, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static async Task<long> SendAsync(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
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
        public static string GetServiceBusName(this IServiceBus serviceBusEntity, Installation installation = Installation.Default)
        {
            return Instance(serviceBusEntity.GetType(), installation).QueueName;
        }
        public static DebugMessage DebugSend(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return serviceBusEntity.DebugSendAsync(delayInSeconds, installation, attempt, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static async Task<DebugMessage> DebugSendAsync(this IServiceBus serviceBusEntity, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
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
        public static bool Delete(this IServiceBus serviceBusEntity, long messageId, Installation installation = Installation.Default)
        {
            return serviceBusEntity.DeleteAsync(messageId, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static async Task<bool> DeleteAsync(this IServiceBus serviceBusEntity, long messageId, Installation installation = Installation.Default)
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
        public static long SendBatch(this IEnumerable<IServiceBus> serviceBusEntities, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return serviceBusEntities.SendBatchAsync(delayInSeconds, installation, attempt, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static async Task<long> SendBatchAsync(this IEnumerable<IServiceBus> serviceBusEntities, int delayInSeconds = 0, Installation installation = Installation.Default, int attempt = 0, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            Message message = new Message(Encoding.UTF8.GetBytes(new ServiceBusMessage()
            {
                Attempt = attempt,
                Container = serviceBusEntities,
                Flow = flowType,
                Version = version,
                Installation = installation,
                IsBatch = serviceBusEntities.Count()
            }.ToJson()));
            if (delayInSeconds == 0)
                await Instance(serviceBusEntities.FirstOrDefault().GetType(), installation).SendAsync(message);
            else
                return await Instance(serviceBusEntities.FirstOrDefault().GetType(), installation).ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delayInSeconds));
            return 0;
        }
  
        private static Dictionary<string, QueueClient> servicesBus = new Dictionary<string, QueueClient>();
        private static readonly object TrafficLight = new object();
        private static QueueClient Instance(Type type, Installation installation = Installation.Default)
        {
            string key = installation == Installation.Default ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!servicesBus.ContainsKey(key))
                lock (TrafficLight)
                {
                    if (!servicesBus.ContainsKey(key))
                        servicesBus.Add(key,
                            new QueueClient(
                                new ServiceBusConnectionStringBuilder(
                                    ServiceBusInstaller.GetCompleteConnectionStringAndEntityPath(type, installation)),
                                ReceiveMode.PeekLock));
                }
            return servicesBus[key];
        }
    }
}
