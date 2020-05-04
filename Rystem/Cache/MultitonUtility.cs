using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    public static class MultitonUtility
    {
        public static bool ClearAllCache(string connectionString) 
            => ClearAllCacheAsync(connectionString).ToResult();
        public static async Task<bool> ClearAllCacheAsync(string connectionString)
        {
            ConnectionMultiplexer Connection = ConnectionMultiplexer.Connect(connectionString);
            IDatabase cache = Connection.GetDatabase();
            foreach (RedisKey redisKey in cache.Multiplexer.GetServer(cache.Multiplexer.GetEndPoints().First()).Keys())
                await cache.KeyDeleteAsync(redisKey).NoContext();
            return true;
        }
        public static bool ClearAllTableStorage(string connectionString) 
            => ClearAllTableStorageAsync(connectionString).ToResult();
#warning Alessandro Rapiti - da terminare
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static async Task<bool> ClearAllTableStorageAsync(string connectionString)
        {
            await Task.Delay(0).NoContext();
            throw new NotImplementedException();
        }
    }
}
