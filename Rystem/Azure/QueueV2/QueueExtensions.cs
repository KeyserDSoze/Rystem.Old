using Newtonsoft.Json;
using Rystem.Azure.Queue;
using Rystem.Const;
using Rystem.Debug;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static partial class QueueExtensions
    {
        private readonly static Dictionary<string, IQueueManager> Managers = new Dictionary<string, IQueueManager>();
        private readonly static object TrafficLight = new object();
        private static IQueueManager Manager<TEntity>()
            where TEntity : IQueue, new()
        {
            string name = typeof(TEntity).FullName;
            if (!Managers.ContainsKey(name))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(name))
                        Managers.Add(name, new QueueManager<TEntity>());
            return Managers[name];
        }
        public static async Task<bool> SendAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().SendAsync(message, installation, path, organization).NoContext();
        public static async Task<long> SendScheduledAsync<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
           => await Manager<TEntity>().SendScheduledAsync(message, delayInSeconds, installation, path, organization).NoContext();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<bool> DeleteScheduledAsync<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().DeleteScheduledAsync(messageId, installation).NoContext();
        public static async Task<bool> SendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().SendBatchAsync(messages.Select(x => x as IQueue), installation, path, organization).NoContext();
        public static async Task<IEnumerable<long>> SendScheduledBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().SendScheduledBatchAsync(messages.Select(x => x as IQueue), delayInSeconds, installation, path, organization).NoContext();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<IEnumerable<TEntity>> ReadAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().ReadAsync<TEntity>(installation, path, organization).NoContext();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<bool> CleanAsync<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().CleanAsync(installation).NoContext();
        public static async Task<DebugMessage> DebugSendAsync<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().DebugSendAsync(message, delayInSeconds, installation, path, organization).NoContext();
        public static async Task<DebugMessage> DebugSendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => await Manager<TEntity>().DebugSendBatchAsync(messages.Select(x => x as IQueue), delayInSeconds, installation, path, organization).NoContext();

        public static bool Send<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
           => message.SendAsync(path, organization, installation).ToResult();
        public static long SendScheduled<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => message.SendScheduledAsync(delayInSeconds, path, organization, installation).ToResult();
        public static bool DeleteScheduled<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => message.DeleteScheduledAsync(messageId, installation).ToResult();
        public static bool SendBatch<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => messages.SendBatchAsync(path, organization, installation).ToResult();
        public static IEnumerable<long> SendScheduledBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => messages.SendScheduledBatchAsync(delayInSeconds, path, organization, installation).ToResult();
        public static IEnumerable<TEntity> Read<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => message.ReadAsync(path, organization, installation).ToResult();
        public static bool Clean<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => message.CleanAsync(installation).ToResult();
        public static DebugMessage DebugSend<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => message.DebugSendAsync(delayInSeconds, path, organization, installation).ToResult();
        public static DebugMessage DebugSendBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => messages.DebugSendBatchAsync(delayInSeconds, path, organization, installation).ToResult();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static string GetName<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue, new()
            => Manager<TEntity>().GetName(installation);

        public static string ToJson<TEntry>(this TEntry message)
            where TEntry : IQueue, new()
           => message.ToStandardJson();
        public static byte[] ToSendable<TEntry>(this TEntry message)
            where TEntry : IQueue, new()
            => Encoding.UTF8.GetBytes(message.ToJson());
        public static string ToJson<TEntry>(this IEnumerable<TEntry> messages)
            where TEntry : IQueue, new()
            => messages.ToStandardJson();
        public static byte[] ToSendable<TEntry>(this IEnumerable<TEntry> messages)
            where TEntry : IQueue, new()
            => Encoding.UTF8.GetBytes(messages.ToJson());
        internal static TEntity ToMessage<TEntity>(this string message)
            where TEntity : IQueue, new()
            => message.FromStandardJson<TEntity>();
        internal static IEnumerable<TEntity> FromMessage<TEntity>(this IEnumerable<string> messages)
            where TEntity : IQueue, new()
        {
            IList<TEntity> entities = new List<TEntity>();
            foreach (string message in messages)
                entities.Add(message.ToMessage<TEntity>());
            return entities;
        }
    }
}
