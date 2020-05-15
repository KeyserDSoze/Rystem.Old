using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    public static class FastCacheExtensions
    {
        public async static Task SetCacheAsync<T>(this T entity, string key, TimeSpan expiringTime = default)
            => await new FastCacheKey() { Key = key }.RestoreAsync(new FastCache() { Value = entity.ToDefaultJson() }, expiringTime).NoContext();
        public async static Task<T> FromCacheAsync<T>(this string key)
        {
            FastCache fastCache = await new FastCacheKey() { Key = key }.InstanceAsync().NoContext();
            if (fastCache == null)
                return default;
            return fastCache.Value.FromDefaultJson<T>();
        }
        public async static Task<bool> RemoveCacheAsync(this string key)
            => await new FastCacheKey() { Key = key }.RemoveAsync().NoContext();
        public async static Task<bool> ExistCacheAsync(this string key)
            => await new FastCacheKey() { Key = key }.IsPresentAsync().NoContext();

        public static void SetCache<T>(this T entity, string key, TimeSpan expiringTime = default)
            => entity.SetCacheAsync(key, expiringTime).ToResult();
        public static T FromCache<T>(this string key)
            => key.FromCacheAsync<T>().ToResult();
        public static bool RemoveCache(this string key)
            => key.RemoveCacheAsync().ToResult();
        public static bool ExistCache(this string key)
            => key.ExistCacheAsync().ToResult();
    }
}
