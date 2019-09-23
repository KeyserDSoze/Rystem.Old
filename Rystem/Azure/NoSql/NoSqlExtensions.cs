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
        private static Dictionary<string, INoSqlManager> Managers = new Dictionary<string, INoSqlManager>();
        private readonly static object TrafficLight = new object();
        private static INoSqlManager Manager<TEntity>()
            where TEntity : INoSqlStorage
        {
            Type type = typeof(TEntity);
            if (!Managers.ContainsKey(type.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(type.FullName))
                        Managers.Add(type.FullName, new NoSqlManager<TEntity>());
            return Managers[type.FullName];
        }
        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => await Manager<TEntity>().UpdateAsync(entity);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => await Manager<TEntity>().DeleteAsync(entity);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => await Manager<TEntity>().ExistsAsync(entity);
        public static async Task<IEnumerable<TEntity>> GetAsync<TEntity>(this TEntity entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            where TEntity : INoSqlStorage
           => await Manager<TEntity>().FetchAsync<TEntity>(entity, expression, takeCount);

        public static bool Update<TEntity>(this TEntity entity)
           where TEntity : INoSqlStorage
          => UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Delete<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static bool Exists<TEntity>(this TEntity entity)
            where TEntity : INoSqlStorage
           => ExistsAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
        public static IEnumerable<TEntity> Get<TEntity>(this TEntity entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            where TEntity : INoSqlStorage
           => GetAsync(entity, expression, takeCount).ConfigureAwait(false).GetAwaiter().GetResult();
#warning Fare la stessa cosa fatta per AggregatedData
    }
}
