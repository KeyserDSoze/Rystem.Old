using Newtonsoft.Json;
using Rystem;
using Rystem.Queue;
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
        private static IManager<TEntity> GetQueueManager<TEntity>(TEntity entity)
            where TEntity : IQueue
            => new QueueManager<TEntity>(entity.GetConfigurationBuilder(), entity);
        private static IQueueManager<TEntity> Manager<TEntity>(this TEntity entity)
            where TEntity : IQueue
            => entity.DefaultManager<TEntity>(GetQueueManager) as IQueueManager<TEntity>;
        public static async Task<bool> SendAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await message.Manager().SendAsync(message, installation, path, organization).NoContext();
        public static async Task<long> SendScheduledAsync<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
           => await message.Manager().SendScheduledAsync(message, delayInSeconds, installation, path, organization).NoContext();

        public static async Task<bool> DeleteScheduledAsync<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await message.Manager().DeleteScheduledAsync(messageId, installation).NoContext();
        public static async Task<bool> SendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
        {
            bool result = true;
            foreach (var msgs in messages.GroupBy(x => x.GetType().FullName))
                result &= await msgs.FirstOrDefault().Manager().SendBatchAsync(msgs, installation, path, organization).NoContext();
            return result;
        }

        public static async Task<IEnumerable<long>> SendScheduledBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
        {
            List<long> aggregatedResponse = new List<long>();
            foreach (var msgs in messages.GroupBy(x => x.GetType().FullName))
                aggregatedResponse.AddRange(await msgs.FirstOrDefault().Manager().SendScheduledBatchAsync(msgs, delayInSeconds, installation, path, organization).NoContext());
            return aggregatedResponse;
        }

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<IEnumerable<TEntity>> ReadAsync<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await message.Manager().ReadAsync(installation, path, organization).NoContext();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<bool> CleanAsync<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await message.Manager().CleanAsync(installation).NoContext();
        public static async Task<DebugMessage> DebugSendAsync<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await message.Manager().DebugSendAsync(message, delayInSeconds, installation, path, organization).NoContext();
        public static async Task<DebugMessage> DebugSendBatchAsync<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => await messages.FirstOrDefault().Manager().DebugSendBatchAsync(messages, delayInSeconds, installation, path, organization).NoContext();

        public static bool Send<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
           => message.SendAsync(path, organization, installation).ToResult();
        public static long SendScheduled<TEntity>(this TEntity message, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.SendScheduledAsync(delayInSeconds, path, organization, installation).ToResult();
        public static bool DeleteScheduled<TEntity>(this TEntity message, long messageId, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.DeleteScheduledAsync(messageId, installation).ToResult();
        public static bool SendBatch<TEntity>(this IEnumerable<TEntity> messages, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.SendBatchAsync(path, organization, installation).ToResult();
        public static IEnumerable<long> SendScheduledBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.SendScheduledBatchAsync(delayInSeconds, path, organization, installation).ToResult();
        public static IEnumerable<TEntity> Read<TEntity>(this TEntity message, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.ReadAsync(path, organization, installation).ToResult();
        public static bool Clean<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.CleanAsync(installation).ToResult();
        public static DebugMessage DebugSend<TEntity>(this TEntity message, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.DebugSendAsync(delayInSeconds, path, organization, installation).ToResult();
        public static DebugMessage DebugSendBatch<TEntity>(this IEnumerable<TEntity> messages, int delayInSeconds = 0, int path = 0, int organization = 0, Installation installation = Installation.Default)
            where TEntity : IQueue
            => messages.DebugSendBatchAsync(delayInSeconds, path, organization, installation).ToResult();

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static string GetName<TEntity>(this TEntity message, Installation installation = Installation.Default)
            where TEntity : IQueue
            => message.Manager().GetName(installation);

        public static byte[] ToSendable<TEntry>(this TEntry message)
            => Encoding.UTF8.GetBytes(message.ToDefaultJson());
        internal static TEntity ToMessage<TEntity>(this string message)
            => message.FromDefaultJson<TEntity>();
    }
}
