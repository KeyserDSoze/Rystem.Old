using Microsoft.Azure.EventHubs;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public static class EventHubExtension
    {
        #region Async Methods
        public static async Task<bool> SendAsync(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
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
        public static async Task<bool> SendBatchAsync(this IEnumerable<IEventHub> eventHubEntities, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            EventHubClient eventHubClient = Instance(eventHubEntities.FirstOrDefault().GetType(), installation);
            EventDataBatch eventDataBatch = eventHubClient.CreateBatch();
            foreach (IEventHub eventHubEntity in eventHubEntities)
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
                eventDataBatch.TryAdd(eventData);
            }
            await eventHubClient.SendAsync(eventDataBatch);
            return true;
        }
        public static async Task<DebugMessage> DebugSendAsync(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            await Task.Delay(0);
            if (Instance(eventHubEntity.GetType(), installation) == null)
                throw new NotImplementedException("Please insert a correct connection string and entity path");
            else
                return new DebugMessage()
                {
                    EventDatas = new EventData[1]
                    {
                        new EventData(Encoding.UTF8.GetBytes(new EventHubMessage()
                        {
                            Attempt = attempt,
                            Container = eventHubEntity,
                            Flow = flowType,
                            Version = version,
                            Installation = installation
                        }.ToJson()))
                    }
                };
        }
        public static async Task<DebugMessage> DebugSendBatchAsync(this IEnumerable<IEventHub> eventHubEntities, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            await Task.Delay(0);
            if (Instance(eventHubEntities.FirstOrDefault().GetType(), installation) == null)
                throw new NotImplementedException("Please insert a correct connection string and entity path");
            else
            {
                List<EventData> eventDatas = new List<EventData>();
                foreach (IEventHub eventHubEntity in eventHubEntities)
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
                    eventDatas.Add(eventData);
                }
                return new DebugMessage()
                {
                    EventDatas = eventDatas.ToArray()
                };
            }
        }
        #endregion
        #region Sync Methods
        public static bool Send(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return eventHubEntity.SendAsync(attempt, installation, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static bool SendBatch(this IEnumerable<IEventHub> eventHubEntities, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return eventHubEntities.SendBatchAsync(attempt, installation, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static string GetEventHubName(this IEventHub eventHubEntity, Installation installation = Installation.Default)
        {
            return Instance(eventHubEntity.GetType(), installation).EventHubName;
        }
        public static DebugMessage DebugSend(this IEventHub eventHubEntity, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return eventHubEntity.DebugSendAsync(attempt, installation, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static DebugMessage DebugSendBatch(this IEnumerable<IEventHub> eventHubEntities, int attempt = 0, Installation installation = Installation.Default, FlowType flowType = FlowType.Flow0, VersionType version = VersionType.V0)
        {
            return eventHubEntities.DebugSendBatchAsync(attempt, installation, flowType, version).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        #endregion
        private static Dictionary<string, EventHubClient> eventHubs = new Dictionary<string, EventHubClient>();
        private static readonly object TrafficLight = new object();
        private static EventHubClient Instance(Type type, Installation installation = Installation.Default)
        {
            string key = installation == Installation.Default ? $"{type.FullName}" : $"{type.FullName}_{installation}";
            if (!eventHubs.ContainsKey(key))
                lock (TrafficLight)
                {
                    if (!eventHubs.ContainsKey(key))
                        eventHubs.Add(key, EventHubClient.CreateFromConnectionString(
                            new EventHubsConnectionStringBuilder(EventHubInstaller.GetCompleteConnectionStringAndEntityPath(type)).ToString()));
                }
            return eventHubs[key];
        }
    }
}
