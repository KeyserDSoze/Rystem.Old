using Newtonsoft.Json;
using Rystem.Cache;
using Rystem.Const;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Cache
{
    internal class InRedisCache<T> : IMultitonIntegration<T>
        where T : IMultiton, new()
    {
        private int RoundRobin = -1;
        private static readonly object TrafficLight = new object();
        private IDatabase Cache
        {
            get
            {
                int value = 0;
                if (this.Configuration.NumberOfClients > 1)
                    lock (TrafficLight)
                        value = this.RoundRobin = (this.RoundRobin + 1) % this.Configuration.NumberOfClients;
                return Connections[value].Value.GetDatabase();
            }
        }

        private readonly List<Lazy<ConnectionMultiplexer>> Connections;
        private readonly TimeSpan ExpireCache;
        private readonly string FullName = typeof(T).FullName;
        private readonly InCloudMultitonProperties Configuration;
        internal InRedisCache(InCloudMultitonProperties configuration)
        {
            Configuration = configuration;
            ExpireCache = configuration.ExpireTimeSpan;
            Connections = new List<Lazy<ConnectionMultiplexer>>();
            for (int i = 0; i < configuration.NumberOfClients; i++)
                Connections.Add(new Lazy<ConnectionMultiplexer>(() =>
                {
                    ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.ConnectionString);
                    return connectionMultiplexer;
                }));
        }
        public T Instance(string key)
        {
            string json = Cache.StringGet(CloudKeyToString(key));
            return json.FromDefaultJson<T>();
        }
        public bool Update(string key, T value, TimeSpan expiringTime)
        {
            bool code;
            if (expiringTime == default)
                expiringTime = ExpireCache;
            if (expiringTime.Ticks > 0)
                code = Cache.StringSet(CloudKeyToString(key), value.ToDefaultJson(), expiringTime);
            else
                code = Cache.StringSet(CloudKeyToString(key), value.ToDefaultJson());
            return code;
        }
        public bool Exists(string key)
            => Cache.KeyExists(CloudKeyToString(key));
        public bool Delete(string key)
            => Cache.KeyDelete(CloudKeyToString(key));
        public IEnumerable<string> List()
        {
            string toReplace = $"{FullName}{MultitonConst.Separator}";
            List<string> keys = new List<string>();
            foreach (string redisKey in Cache.Multiplexer.GetServer(Cache.Multiplexer.GetEndPoints().First()).Keys())
                if (redisKey.Contains(toReplace))
                    keys.Add(redisKey.Replace(toReplace, string.Empty));
            return keys;
        }
        private string CloudKeyToString(string keyString)
           => $"{FullName}{MultitonConst.Separator}{keyString}";
    }
}
