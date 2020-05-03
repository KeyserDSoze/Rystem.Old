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
        private static IAggregatedDataManager Manager<TEntity>()
            where TEntity : IAggregatedData, new()
        {
            string name = typeof(TEntity).FullName;
            if (!Managers.ContainsKey(name))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(name))
                        Managers.Add(name, new AggregatedDataManager<TEntity>());
            return Managers[name];
        }
        public static async Task<bool> WriteAsync<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => await Manager<TEntity>().WriteAsync(entity, installation, offset).NoContext();
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().DeleteAsync(entity, installation).NoContext();
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().ExistsAsync(entity, installation).NoContext();
        public static async Task<TEntity> FetchAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().FetchAsync<TEntity>(entity, installation).NoContext();
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().ListAsync<TEntity>(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().SearchAsync(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<AggregatedDataDummy>> FetchPropertiesAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => await Manager<TEntity>().FetchPropertiesAsync(entity, installation, prefix, takeCount).NoContext();

        public static bool Write<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => WriteAsync(entity, offset, installation).NoContext().GetAwaiter().GetResult();
        public static TEntity Fetch<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IAggregatedData, new()
           => FetchAsync(entity, installation).NoContext().GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => DeleteAsync(entity, installation).NoContext().GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
            => ExistsAsync(entity, installation).NoContext().GetAwaiter().GetResult();
        public static IEnumerable<TEntity> List<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => ListAsync<TEntity>(entity, prefix, takeCount, installation).NoContext().GetAwaiter().GetResult();
        public static IList<string> Search<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => SearchAsync(entity, prefix, takeCount, installation).NoContext().GetAwaiter().GetResult();
        public static IList<AggregatedDataDummy> FetchProperties<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IAggregatedData, new()
           => FetchPropertiesAsync(entity, prefix, takeCount, installation).NoContext().GetAwaiter().GetResult();
#pragma warning disable IDE0060 // Remove unused parameter
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IAggregatedData, new()
            => Manager<TEntity>().GetName(installation);
#pragma warning restore IDE0060 // Remove unused parameter
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
