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

        public static string ToJson(this IQueue message)
            => JsonConvert.SerializeObject(message, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IQueue message)
            => Encoding.UTF8.GetBytes(message.ToJson());

        public static string ToJson(this IEnumerable<IQueue> messages)
            => JsonConvert.SerializeObject(messages, ExternalLibrarySettings.JsonSettings);
        public static byte[] ToSendable(this IEnumerable<IQueue> messages)
            => Encoding.UTF8.GetBytes(messages.ToJson());

        public static async Task<bool> SendAsync<TEntity>(this TEntity message)
            where TEntity : IQueue
            => await Manager(message.GetType()).SendAsync(message);
        public static async Task<long> SendScheduledAsync(this IQueue message, int delayInSeconds)
            => await Manager(message.GetType()).SendScheduledAsync(message, delayInSeconds);
        public static async Task<bool> DeleteScheduledAsync(this IQueue message, long messageId)
            => await Manager(message.GetType()).DeleteScheduledAsync(messageId);
        public static async Task<bool> SendBatchAsync(this IEnumerable<IQueue> messages)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendBatchAsync(messages);
        public static async Task<IList<long>> SendScheduledBatchAsync(this IEnumerable<IQueue> messages, int delayInSeconds)
            => await Manager(messages.FirstOrDefault()?.GetType()).SendScheduledBatchAsync(messages, delayInSeconds);
        public static async Task<DebugMessage> DebugSendAsync(this IQueue message, int delayInSeconds = 0)
            => await Manager(message.GetType()).DebugSendAsync(message, delayInSeconds);
        public static async Task<DebugMessage> DebugSendBatchAsync(this IEnumerable<IQueue> messages, int delayInSeconds = 0)
            => await Manager(messages.FirstOrDefault()?.GetType()).DebugSendBatchAsync(messages, delayInSeconds);

        public static bool Send(this IQueue message)
           => message.SendAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        public static long SendScheduled(this IQueue message, int delayInSeconds)
            => message.SendScheduledAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool DeleteScheduled(this IQueue message, long messageId)
            => message.DeleteScheduledAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool SendBatch(this IEnumerable<IQueue> messages)
            => messages.SendBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<long> SendScheduledBatch(this IEnumerable<IQueue> messages, int delayInSeconds)
            => messages.SendScheduledBatchAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSend(this IQueue message, int delayInSeconds = 0)
            => message.DebugSendAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSendBatch(this IEnumerable<IQueue> messages, int delayInSeconds = 0)
            => messages.DebugSendBatchAsync(delayInSeconds).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string GetName(this IQueue message)
            => Manager(message.GetType()).GetName();
    }
}
