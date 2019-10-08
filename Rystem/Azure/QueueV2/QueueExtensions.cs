﻿using Newtonsoft.Json;
using Rystem.Azure.Queue;
using Rystem.Debug;
using Rystem.Enums;
using Rystem.Interfaces.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class QueueExtensions
    {
        private static Dictionary<string, IQueueManager> Managers = new Dictionary<string, IQueueManager>();
        private readonly static object TrafficLight = new object();
        private static IQueueManager Manager(Type messageType)
        {
            if (!Managers.ContainsKey(messageType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(messageType.FullName))
                    {
                        Type genericType = typeof(QueueManager<>).MakeGenericType(messageType);
                        Managers.Add(messageType.FullName, (IQueueManager)Activator.CreateInstance(genericType));
                    }
            return Managers[messageType.FullName];
        }

        public static string ToJson(this IQueue message)
            => JsonConvert.SerializeObject(message, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IQueue message)
            => Encoding.UTF8.GetBytes(message.ToJson());
        public static TEntity FromString<TEntity>(this string message)
            where TEntity : IQueue
            => JsonConvert.DeserializeObject<TEntity>(message);
        public static string ToJson(this IEnumerable<IQueue> messages)
            => JsonConvert.SerializeObject(messages, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IEnumerable<IQueue> messages)
            => Encoding.UTF8.GetBytes(messages.ToJson());

        public static async Task<bool> SendAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager(message.GetType()).SendAsync(message, installation, path, organization);
        public static async Task<long> SendScheduledAsync(this IQueue message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => await Manager(message.GetType()).SendScheduledAsync(message, delayInSeconds, installation, path, organization);
        public static async Task<bool> DeleteScheduledAsync(this IQueue message, long messageId, Installation installation = Installation.Default)
            => await Manager(message.GetType()).DeleteScheduledAsync(messageId, installation);
        public static async Task<bool> SendBatchAsync(this IEnumerable<IQueue> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendBatchAsync(messages, installation, path, organization);
        public static async Task<IList<long>> SendScheduledBatchAsync(this IEnumerable<IQueue> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendScheduledBatchAsync(messages, delayInSeconds, installation, path, organization);
        public static async Task<IEnumerable<TEntity>> ReadAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager(message.GetType()).ReadAsync<TEntity>(installation, path, organization);
        public static async Task<DebugMessage> DebugSendAsync(this IQueue message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => await Manager(message.GetType()).DebugSendAsync(message, delayInSeconds, installation, path, organization);
        public static async Task<DebugMessage> DebugSendBatchAsync(this IEnumerable<IQueue> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => await Manager(messages.FirstOrDefault()?.GetType()).DebugSendBatchAsync(messages, delayInSeconds, installation, path, organization);

        public static bool Send(this IQueue message, int path = 0, int organization = 0, Installation installation = Installation.Default)
           => message.SendAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static long SendScheduled(this IQueue message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => message.SendScheduledAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool DeleteScheduled(this IQueue message, long messageId, Installation installation = Installation.Default)
            => message.DeleteScheduledAsync(messageId, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool SendBatch(this IEnumerable<IQueue> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => messages.SendBatchAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<long> SendScheduledBatch(this IEnumerable<IQueue> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => messages.SendScheduledBatchAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> Read<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.ReadAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSend(this IQueue message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => message.DebugSendAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSendBatch(this IEnumerable<IQueue> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            => messages.DebugSendBatchAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string GetName(this IQueue message, Installation installation = Installation.Default)
            => Manager(message.GetType()).GetName(installation);
    }
}
