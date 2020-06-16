using Rystem;
using Rystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DataExtensions
    {
        private static IManager<TEntity> GetDataManager<TEntity>(TEntity entity)
            where TEntity : IData
            => new DataManager<TEntity>(entity.GetConfigurationBuilder(), entity);
        private static IDataManager<TEntity> Manager<TEntity>(this TEntity entity)
            where TEntity : IData
            => entity.DefaultManager<TEntity>(GetDataManager) as IDataManager<TEntity>;

        public static async Task<bool> WriteAsync<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IData
            => await entity.Manager().WriteAsync(entity, installation, offset).NoContext();
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().DeleteAsync(entity, installation).NoContext();
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().ExistsAsync(entity, installation).NoContext();
        public static async Task<TEntity> FetchAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().FetchAsync(entity, installation).NoContext();
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().ListAsync(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().SearchAsync(entity, installation, prefix, takeCount).NoContext();
        public static async Task<IList<DataWrapper>> FetchPropertiesAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.Manager().FetchPropertiesAsync(entity, installation, prefix, takeCount).NoContext();

        public static bool Write<TEntity>(this TEntity entity, long offset = 0, Installation installation = Installation.Default)
            where TEntity : IData
            => WriteAsync(entity, offset, installation).ToResult();
        public static TEntity Fetch<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IData
           => FetchAsync(entity, installation).ToResult();
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IData
            => DeleteAsync(entity, installation).ToResult();
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : IData
            => ExistsAsync(entity, installation).ToResult();
        public static IEnumerable<TEntity> List<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => ListAsync(entity, prefix, takeCount, installation).ToResult();
        public static IList<string> Search<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => SearchAsync(entity, prefix, takeCount, installation).ToResult();
        public static IList<DataWrapper> FetchProperties<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null, Installation installation = Installation.Default)
            where TEntity : IData
           => FetchPropertiesAsync(entity, prefix, takeCount, installation).ToResult();
        public static string GetName<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : IData
            => entity.Manager().GetName(installation);
    }

    public static class DataLinqExtensions
    {
        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IData
           => (await entity.ListAsync(prefix, 1, installation).NoContext()).FirstOrDefault();
        public static async Task<IList<TEntity>> ToListAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IData
           => (await entity.ListAsync(prefix, null, installation).NoContext()).ToList();
        public static async Task<IEnumerable<TEntity>> TakeAsync<TEntity>(this TEntity entity, int takeCount, string prefix = null, Installation installation = Installation.Default)
            where TEntity : IData
           => await entity.ListAsync(prefix, takeCount, installation).NoContext();
        public static async Task<int> CountAsync<TEntity>(this TEntity entity, string prefix = null, Installation installation = Installation.Default)
           where TEntity : IData
          => (await entity.SearchAsync(prefix, null, installation).NoContext()).Count;
    }
}
