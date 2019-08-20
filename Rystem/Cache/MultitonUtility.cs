using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    public static class MultitonUtility
    {
        public static async Task<bool> ClearAllCacheAsync(string connectionString)
        {
            ConnectionMultiplexer Connection = ConnectionMultiplexer.Connect(connectionString);
            IDatabase cache = Connection.GetDatabase();
            foreach (RedisKey redisKey in cache.Multiplexer.GetServer(cache.Multiplexer.GetEndPoints().First()).Keys())
                await cache.KeyDeleteAsync(redisKey);
            return true;
        }
        public static async Task<bool> ClearAllTableStorageAsync(string connectionString)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}
