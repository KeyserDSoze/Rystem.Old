using Newtonsoft.Json;
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
    public static partial class QueueExtensions
    {
        private static Dictionary<string, IQueueManager> Managers = new Dictionary<string, IQueueManager>();
        private readonly static object TrafficLight = new object();
        private static IQueueManager Manager<TEntity>()
            where TEntity : IQueue
        {
            string name = typeof(TEntity).FullName;
            if (!Managers.ContainsKey(name))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(name))
                        Managers.Add(name, new QueueManager<TEntity>());
            return Managers[name];
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
            => await Manager<TEntity>().SendAsync(message, installation, path, organization);
        public static async Task<long> SendScheduledAsync<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
           => await Manager<TEntity>().SendScheduledAsync(message, delayInSeconds, installation, path, organization);
        public static async Task<bool> DeleteScheduledAsync<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().DeleteScheduledAsync(messageId, installation);
        public static async Task<bool> SendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().SendBatchAsync(messages.Select(x => x as IQueue), installation, path, organization);
        public static async Task<IList<long>> SendScheduledBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().SendScheduledBatchAsync(messages.Select(x => x as IQueue), delayInSeconds, installation, path, organization);
        public static async Task<IEnumerable<TEntity>> ReadAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().ReadAsync<TEntity>(installation, path, organization);
        public static async Task<bool> CleanAsync<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().CleanAsync(installation);
        public static async Task<DebugMessage> DebugSendAsync<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().DebugSendAsync(message, delayInSeconds, installation, path, organization);
        public static async Task<DebugMessage> DebugSendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await Manager<TEntity>().DebugSendBatchAsync(messages.Select(x => x as IQueue), delayInSeconds, installation, path, organization);

        public static bool Send<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
           => message.SendAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static long SendScheduled<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.SendScheduledAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool DeleteScheduled<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.DeleteScheduledAsync(messageId, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool SendBatch<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.SendBatchAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<long> SendScheduledBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.SendScheduledBatchAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> Read<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.ReadAsync(path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Clean<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.CleanAsync(installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSend<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.DebugSendAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static DebugMessage DebugSendBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.DebugSendBatchAsync(delayInSeconds, path, organization, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string GetName<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => Manager<TEntity>().GetName(installation);
    }
}
