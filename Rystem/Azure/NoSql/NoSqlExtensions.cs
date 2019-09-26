using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class NoSqlExtensions
    {
        private static Dictionary<string, INoSqlManager> Managers = new Dictionary<string, INoSqlManager>();
        private readonly static object TrafficLight = new object();
        private static INoSqlManager Manager(Type messageType)
        {
            if (!Managers.ContainsKey(messageType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(messageType.FullName))
                    {
                        Type genericType = typeof(NoSqlManager<>).MakeGenericType(messageType);
                        Managers.Add(messageType.FullName, (INoSqlManager)Activator.CreateInstance(genericType));
                    }
            return Managers[messageType.FullName];
        }
        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity)
            where TEntity : INoSql
           => await Manager(entity.GetType()).UpdateAsync(entity);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : INoSql
           => await Manager(entity.GetType()).DeleteAsync(entity);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity)
            where TEntity : INoSql
           => await Manager(entity.GetType()).ExistsAsync(entity);
        public static async Task<IEnumerable<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null)
            where TEntity : INoSql
           => await Manager(entity.GetType()).FetchAsync<TEntity>(entity, expression, takeCount);

        public static bool Update<TEntity>(this TEntity entity)
           where TEntity : INoSql
          => UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity)
            where TEntity : INoSql
           => DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity)
            where TEntity : INoSql
           => ExistsAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null)
            where TEntity : INoSql
           => GetAsync(entity, expression, takeCount).ConfigureAwait(false).GetAwaiter().GetResult();
#warning Fare la stessa cosa fatta per AggregatedData
    }
}
