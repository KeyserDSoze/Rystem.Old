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
        public static async Task<bool> AppendAsync<TEntity>(this TEntity entity, long offset = 0)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).AppendAsync(entity, offset);
        public static async Task<string> WriteAsync<TEntity>(this TEntity entity)
        where TEntity : IAggregatedData
       => await Manager(entity.GetType()).WriteAsync(entity);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).DeleteAsync(entity);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).ExistsAsync(entity);
        public static async Task<TEntity> FetchAsync<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData
           => await Manager(entity.GetType()).FetchAsync<TEntity>(entity);
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IAggregatedData
           => (await Manager(entity.GetType()).ListAsync<TEntity>(entity, prefix, takeCount));
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IAggregatedData
           => (await Manager(entity.GetType()).SearchAsync(entity, prefix, takeCount));

        public static bool Append<TEntity>(this TEntity entity, long offset = 0)
            where TEntity : IAggregatedData
           => AppendAsync(entity, offset).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string Write<TEntity>(this TEntity entity)
        where TEntity : IAggregatedData
       => WriteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static TEntity Fetch<TEntity>(this TEntity entity)
       where TEntity : IAggregatedData
      => FetchAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData
           => DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity)
            where TEntity : IAggregatedData
           => ExistsAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> List<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IAggregatedData
           => ListAsync<TEntity>(entity, prefix, takeCount).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<string> Search<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IAggregatedData
           => SearchAsync(entity, prefix, takeCount).ConfigureAwait(false).GetAwaiter().GetResult();
        public static string GetName<TEntity>(this TEntity entity)
           where TEntity : IAggregatedData
       => Manager(entity.GetType()).GetName();
    }
}
