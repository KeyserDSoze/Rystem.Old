﻿using Rystem.Azure.NoSql;

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
        private readonly static Dictionary<string, INoSqlManager> Managers = new Dictionary<string, INoSqlManager>();
        private readonly static object TrafficLight = new object();
        private static INoSqlManager Manager<TEntity>()
            where TEntity : INoSql
        {
            string name = typeof(TEntity).FullName;
            if (!Managers.ContainsKey(name))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(name))
                        Managers.Add(name, new NoSqlManager<TEntity>());
            return Managers[name];
        }
        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().UpdateAsync(entity, installation);
        public static async Task<bool> UpdateBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().UpdateBatchAsync(entities.Select(x => (INoSql)x), installation);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().DeleteAsync(entity, installation);
        public static async Task<bool> DeleteBatchAsync<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().DeleteBatchAsync(entities.Select(x => (INoSql)x), installation);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().ExistsAsync(entity, installation);
        public static async Task<IList<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => await Manager<TEntity>().GetAsync(entity, installation, expression, takeCount);

        public static bool Update<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : INoSql
          => UpdateAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool UpdateBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
           where TEntity : INoSql
          => UpdateBatchAsync(entities, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => DeleteAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool DeleteBatch<TEntity>(this IEnumerable<TEntity> entities, Installation installation = Installation.Default)
           where TEntity : INoSql
          => DeleteBatchAsync(entities, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
           => ExistsAsync(entity, installation).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IList<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : INoSql
           => GetAsync(entity, expression, takeCount, installation).ConfigureAwait(false).GetAwaiter().GetResult();


        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : INoSql
        => Manager<TEntity>().GetName(installation);
    }
}
