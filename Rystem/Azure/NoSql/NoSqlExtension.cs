using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class NoSqlExtension
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
            where TEntity : INoSqlStorage
           => await Manager(entity.GetType()).UpdateAsync(entity);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => await Manager(entity.GetType()).DeleteAsync(entity);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => await Manager(entity.GetType()).ExistsAsync(entity);
        public static async Task<IEnumerable<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            where TEntity : INoSqlStorage
           => (await Manager(entity.GetType()).FetchAsync(entity, expression, takeCount)).Select(x => (TEntity)x);
    }
}
