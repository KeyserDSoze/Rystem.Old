using Rystem.Azure.NoSql;
using Rystem.Enums;
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
        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager(entity.GetType()).UpdateAsync(entity, installation);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager(entity.GetType()).DeleteAsync(entity, installation);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager(entity.GetType()).ExistsAsync(entity, installation);
        public static async Task<IEnumerable<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager(entity.GetType()).FetchAsync<TEntity>(entity, installation, expression, takeCount);

        public static bool Update<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : INoSql
          => UpdateAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => DeleteAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => ExistsAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => GetAsync(entity, expression, takeCount, installation).ConfigureAwait(false).GetAwaiter().GetResult();
#warning Fare la stessa cosa fatta per AggregatedData
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
        => Manager(entity.GetType()).GetName(installation);
    }
}
