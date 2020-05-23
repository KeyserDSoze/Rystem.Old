using Newtonsoft.Json;
using Rystem.Cache;
using Rystem.Const;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal class InRedisCache<T> : IMultitonIntegrationAsync<T>
    {
        private int RoundRobin = -1;
        private static readonly object TrafficLight = new object();
        private IDatabase Cache
        {
            get
            {
                int value = 0;
                if (this.Properties.NumberOfClients > 1)
                    lock (TrafficLight)
                        value = this.RoundRobin = (this.RoundRobin + 1) % this.Properties.NumberOfClients;
                return Connections[value].Value.GetDatabase();
            }
        }
        public Task WarmUp()
        {
            foreach (var connection in Connections)
                _ = connection.Value.GetDatabase();
            return Task.CompletedTask;
        }

        private readonly List<Lazy<ConnectionMultiplexer>> Connections;
        private readonly TimeSpan ExpireCache;
        private readonly string FullName = typeof(T).FullName;
        private readonly CacheProperties Properties;
        internal InRedisCache(RystemCacheProperty configuration)
        {
            Properties = configuration.CloudProperties;
            ExpireCache = Properties.ExpireTimeSpan;
            Connections = new List<Lazy<ConnectionMultiplexer>>();
            for (int i = 0; i < Properties.NumberOfClients; i++)
                Connections.Add(new Lazy<ConnectionMultiplexer>(() =>
                {
                    ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.ConnectionString);
                    return connectionMultiplexer;
                }));
        }
        public async Task<T> InstanceAsync(string key)
        {
            string json = await Cache.StringGetAsync(CloudKeyToString(key)).NoContext();
            return json.FromDefaultJson<T>();
        }
        public async Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime)
        {
            bool code;
            if (expiringTime == default)
                expiringTime = ExpireCache;
            if (expiringTime.Ticks > 0)
                code = await Cache.StringSetAsync(CloudKeyToString(key), value.ToDefaultJson(), expiringTime).NoContext();
            else
                code = await Cache.StringSetAsync(CloudKeyToString(key), value.ToDefaultJson()).NoContext();
            return code;
        }
        public async Task<MultitonStatus<T>> ExistsAsync(string key)
            => await Cache.KeyExistsAsync(CloudKeyToString(key)).NoContext() ? MultitonStatus<T>.Ok() : MultitonStatus<T>.NotOk();
        public async Task<bool> DeleteAsync(string key)
            => await Cache.KeyDeleteAsync(CloudKeyToString(key)).NoContext();
        public Task<IEnumerable<string>> ListAsync()
        {
            string toReplace = $"{FullName}{MultitonConst.Separator}";
            List<string> keys = new List<string>();
            foreach (string redisKey in Cache.Multiplexer.GetServer(Cache.Multiplexer.GetEndPoints().First()).Keys())
                if (redisKey.Contains(toReplace))
                    keys.Add(redisKey.Replace(toReplace, string.Empty));
            return Task.FromResult(keys.Select(x => x));
        }
        private string CloudKeyToString(string keyString)
           => $"{FullName}{MultitonConst.Separator}{keyString}";
    }
}
