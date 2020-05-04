using Rystem.Azure.AggregatedData;

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
        private readonly static Dictionary<string, IAggregatedDataManager> Managers = new Dictionary<string, IAggregatedDataManager>();
        private readonly static object TrafficLight = new object();
        private static IAggregatedDataManager Manager<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData, new()
        {
            Type entityType = entity.GetType();
            if (!Managers.ContainsKey(entityType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(entityType.FullName))
                    {
                        Type genericType = typeof(AggregatedDataManager<>).MakeGenericType(entityType);
                        Managers.Add(entityType.FullName, (IAggregatedDataManager)Activator.CreateInstance(genericType));
                    }
            return Managers[entityType.FullName];
        }
        public static async Task<bool> WriteAsync<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => await entity.Manager().WriteAsync(entity, installation, offset).NoContext();
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().DeleteAsync(entity, installation).NoContext();
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().ExistsAsync(entity, installation).NoContext();
        public static async Task<TEntity> FetchAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().FetchAsync<TEntity>(entity, installation).NoContext();
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().ListAsync<TEntity>(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().SearchAsync(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<AggregatedDataDummy>> FetchPropertiesAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.Manager().FetchPropertiesAsync(entity, installation, prefix, takeCount).NoContext();

        public static bool Write<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => WriteAsync(entity, offset, installation).ToResult();
        public static TEntity Fetch<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IAggregatedData, new()
           => FetchAsync(entity, installation).ToResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => DeleteAsync(entity, installation).ToResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => ExistsAsync(entity, installation).ToResult();
        public static IEnumerable<TEntity> List<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => ListAsync(entity, prefix, takeCount, installation).ToResult();
        public static IList<string> Search<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => SearchAsync(entity, prefix, takeCount, installation).ToResult();
        public static IList<AggregatedDataDummy> FetchProperties<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => FetchPropertiesAsync(entity, prefix, takeCount, installation).ToResult();
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IAggregatedData, new()
            => entity.Manager().GetName(installation);
    }

    public static class AggregatedDataLinqExtensions
    {
        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => (await entity.ListAsync(prefix, 1, installation).NoContext()).FirstOrDefault();
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => (await entity.ListAsync(prefix, null, installation).NoContext()).ToList();
        public static async Task<IEnumerable<TEntity>> TakeAsync<TEntity>(this TEntity entity, int takeCount, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await entity.ListAsync(prefix, takeCount, installation).NoContext();
        public static async Task<int> CountAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
           where TEntity : IAggregatedData, new()
          => (await entity.SearchAsync(prefix, null, installation).NoContext()).Count;
    }
}
