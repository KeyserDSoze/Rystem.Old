using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Fast
{
    public static class FastNoSqlExtensions
    {
        private static INoSqlManager<TEntity> Manager<TEntity>(this TEntity entity)
        {
            Type entityType = entity.GetType();
            if (!ManagerWrapper<TEntity>.Managers.ContainsKey(entityType.FullName))
                lock (ManagerWrapper<TEntity>.TrafficLight)
                    if (!ManagerWrapper<TEntity>.Managers.ContainsKey(entityType.FullName))
                    {
                        //Type genericType = typeof(NoSqlManager<>).MakeGenericType(entityType);
                        ManagerWrapper<TEntity>.Managers.Add(entityType.FullName,
                            new NoSqlManager<TEntity>(FastNoSqlInstaller.Builder, entity));
                        //(INoSqlManager<TEntity>)Activator.CreateInstance(genericType, entity.GetConfigurationBuilder())
                    }
            return ManagerWrapper<TEntity>.Managers[entityType.FullName];
        }

        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           => await entity.Manager().UpdateAsync(entity, installation).NoContext();
        public static async Task<bool> UpdateBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
        {
            bool result = true;
            foreach (var ents in entities.GroupBy(x => x.GetType().FullName))
                result &= await ents.FirstOrDefault().Manager().UpdateBatchAsync(ents, installation).NoContext();
            return result;
        }
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           => await entity.Manager().DeleteAsync(entity, installation).NoContext();
        public static async Task<bool> DeleteBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
        {
            bool result = true;
            foreach (var ents in entities.GroupBy(x => x.GetType().FullName))
                result &= await ents.FirstOrDefault().Manager().DeleteBatchAsync(ents, installation).NoContext();
            return result;
        }
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           => await entity.Manager().ExistsAsync(entity, installation).NoContext();
        public static async Task<IList<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
           => await entity.Manager().GetAsync(entity, installation, expression, takeCount).NoContext();

        public static bool Update<TEntity>(this TEntity entity, Installation installation = Installation.Default)
          => UpdateAsync(entity, installation).ToResult();
        public static bool UpdateBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
          => UpdateBatchAsync(entities, installation).ToResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           => DeleteAsync(entity, installation).ToResult();
        public static bool DeleteBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
          => DeleteBatchAsync(entities, installation).ToResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           => ExistsAsync(entity, installation).ToResult();
        public static IList<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
           => GetAsync(entity, expression, takeCount, installation).ToResult();

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
          => (await entity.GetAsync(expression, 1, installation).NoContext()).FirstOrDefault();
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
           => (await entity.GetAsync(expression, null, installation).NoContext());
        public static async Task<IList<TEntity>> TakeAsync<TEntity>(this TEntity entity, int takeCount, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
           => await entity.GetAsync(expression, takeCount, installation).NoContext();
        public static async Task<int> CountAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
          => (await entity.GetAsync(expression, null, installation).NoContext()).Count;
    }
}
