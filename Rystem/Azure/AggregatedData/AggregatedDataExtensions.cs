using Rystem.Azure.AggregatedData;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class AggregatedDataExtensions
    {
        private static Dictionary<string, IAggregatedDataManager> Managers = new Dictionary<string, IAggregatedDataManager>();
        private readonly static object TrafficLight = new object();
        private static IAggregatedDataManager Manager(Type dataType)
        {
            if (!Managers.ContainsKey(dataType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(dataType.FullName))
                    {
                        Type genericType = typeof(AggregatedDataManager<>).MakeGenericType(dataType);
                        Managers.Add(dataType.FullName, (IAggregatedDataManager)Activator.CreateInstance(genericType));
                    }
            return Managers[dataType.FullName];
        }
        public static async Task<bool> WriteAsync<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
        where TEntity : IAggregatedData
       => await Manager(entity.GetType()).WriteAsync(entity, installation, offset);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).DeleteAsync(entity, installation);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).ExistsAsync(entity, installation);
        public static async Task<TEntity> FetchAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).FetchAsync<TEntity>(entity, installation);
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => (await Manager(entity.GetType()).ListAsync<TEntity>(entity, installation, prefix, takeCount));
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => (await Manager(entity.GetType()).SearchAsync(entity, installation, prefix, takeCount));

        public static bool Write<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
        where TEntity : IAggregatedData
       => WriteAsync(entity, offset, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static TEntity Fetch<TEntity>(this TEntity entity, Installation installation = Installation.Default)
       where TEntity : IAggregatedData
      => FetchAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => DeleteAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => ExistsAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> List<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => ListAsync<TEntity>(entity, prefix, takeCount, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<string> Search<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
           => SearchAsync(entity, prefix, takeCount, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IAggregatedData
       => Manager(entity.GetType()).GetName(installation);
    }
}
