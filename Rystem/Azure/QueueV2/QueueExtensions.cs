using Newtonsoft.Json;
using Rystem.Azure.Queue;
using Rystem.Debug;
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

        public static string ToJson(this IQueueMessage message)
            => JsonConvert.SerializeObject(message, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IQueueMessage message)
            => Encoding.UTF8.GetBytes(message.ToJson());

        public static string ToJson(this IEnumerable<IQueueMessage> messages)
            => JsonConvert.SerializeObject(messages, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IEnumerable<IQueueMessage> messages)
            => Encoding.UTF8.GetBytes(messages.ToJson());

        public static async Task<bool> SendAsync<TEntity>(this TEntity message)
            where TEntity : IQueueMessage
            => await Manager(message.GetType()).SendAsync(message);
        public static async Task<long> SendScheduledAsync(this IQueueMessage message, int delayInSeconds)
            => await Manager(message.GetType()).SendScheduledAsync(message, delayInSeconds);
        public static async Task<bool> DeleteScheduledAsync(this IQueueMessage message, long messageId)
            => await Manager(message.GetType()).DeleteScheduledAsync(messageId);
        public static async Task<bool> SendBatchAsync(this IEnumerable<IQueueMessage> messages)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendBatchAsync(messages);
        public static async Task<IList<long>> SendScheduledBatchAsync(this IEnumerable<IQueueMessage> messages, int delayInSeconds)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendScheduledBatchAsync(messages, delayInSeconds);
        public static async Task<DebugMessage> DebugSendAsync(this IQueueMessage message, int delayInSeconds = 0)
            => await Manager(message.GetType()).DebugSendAsync(message, delayInSeconds);
        public static async Task<DebugMessage> DebugSendBatchAsync(this IEnumerable<IQueueMessage> messages, int delayInSeconds = 0)
            => await Manager(messages.FirstOrDefault()?.GetType()).DebugSendBatchAsync(messages, delayInSeconds);

        public static bool Send(this IQueueMessage message)
           => message.SendAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        public static long SendScheduled(this IQueueMessage message, int delayInSeconds)
            => message.SendScheduledAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool DeleteScheduled(this IQueueMessage message, long messageId)
            => message.DeleteScheduledAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool SendBatch(this IEnumerable<IQueueMessage> messages)
            => messages.SendBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<long> SendScheduledBatch(this IEnumerable<IQueueMessage> messages, int delayInSeconds)
            => messages.SendScheduledBatchAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSend(this IQueueMessage message, int delayInSeconds = 0)
            => message.DebugSendAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSendBatch(this IEnumerable<IQueueMessage> messages, int delayInSeconds = 0)
            => messages.DebugSendBatchAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
