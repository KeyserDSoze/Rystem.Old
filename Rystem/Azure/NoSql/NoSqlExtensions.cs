using Rystem;
using Rystem.NoSql;
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
        private static IManager<TEntity> GetNoSqlManager<TEntity>(TEntity entity)
            where TEntity : INoSql 
            => new NoSqlManager<TEntity>(entity.GetConfigurationBuilder(), entity);
        private static INoSqlManager<TEntity> Manager<TEntity>(this TEntity entity)
            where TEntity : INoSql 
            => entity.DefaultManager<TEntity>(GetNoSqlManager) as INoSqlManager<TEntity>;

        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await entity.Manager().UpdateAsync(entity, installation).NoContext();
        public static async Task<bool> UpdateBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
            where TEntity : INoSql
        {
            bool result = true;
            foreach (var ents in entities.GroupBy(x => x.GetType().FullName))
                result &= await ents.FirstOrDefault().Manager().UpdateBatchAsync(ents, installation).NoContext();
            return result;
        }
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await entity.Manager().DeleteAsync(entity, installation).NoContext();
        public static async Task<bool> DeleteBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
            where TEntity : INoSql
        {
            bool result = true;
            foreach (var ents in entities.GroupBy(x => x.GetType().FullName))
                result &= await ents.FirstOrDefault().Manager().DeleteBatchAsync(ents, installation).NoContext();
            return result;
        }
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await entity.Manager().ExistsAsync(entity, installation).NoContext();
        public static async Task<IList<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await entity.Manager().GetAsync(entity, installation, expression, takeCount).NoContext();

        public static bool Update<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : INoSql
          => UpdateAsync(entity, installation).ToResult();
        public static bool UpdateBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
           where TEntity : INoSql
          => UpdateBatchAsync(entities, installation).ToResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => DeleteAsync(entity, installation).ToResult();
        public static bool DeleteBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
           where TEntity : INoSql
          => DeleteBatchAsync(entities, installation).ToResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => ExistsAsync(entity, installation).ToResult();
        public static IList<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => GetAsync(entity, expression, takeCount, installation).ToResult();


        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
        => entity.Manager<TEntity>().GetName(installation);
    }

    public static class NoSqlLinqExtensions
    {
        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => (await entity.GetAsync(expression, 1, installation).NoContext()).FirstOrDefault();
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => (await entity.GetAsync(expression, null, installation).NoContext());
        public static async Task<IList<TEntity>> TakeAsync<TEntity>(this TEntity entity, int takeCount, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await entity.GetAsync(expression, takeCount, installation).NoContext();
        public static async Task<int> CountAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, Installation installation = Installation.Default)
           where TEntity : INoSql
          => (await entity.GetAsync(expression, null, installation).NoContext()).Count;
    }
}
