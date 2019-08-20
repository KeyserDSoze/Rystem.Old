using Newtonsoft.Json;
using Rystem.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Cache
{
    internal class InRedisCache<T> : AMultitonIntegration<T>
        where T : IMultiton
    {
        private static IDatabase Cache => Connection.Value.GetDatabase();
        private static Lazy<ConnectionMultiplexer> Connection;
        private static TimeSpan ExpireCache;
        private readonly static string FullName = typeof(T).FullName;
        internal InRedisCache(MultitonInstaller.MultitonConfiguration configuration)
        {
            ExpireCache = TimeSpan.FromMinutes(configuration.ExpireCache);
            Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration.ConnectionString));
        }
        internal override T Instance(string key)
        {
            string json = Cache.StringGet(CloudKeyToString(key));
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        internal override bool Update(string key, T value)
        {
            bool code = false;
            if (ExpireCache.Ticks > 0)
                code = Cache.StringSet(CloudKeyToString(key), JsonConvert.SerializeObject(value, MultitonConst.JsonSettings), ExpireCache);
            else
                code = Cache.StringSet(CloudKeyToString(key), JsonConvert.SerializeObject(value, MultitonConst.JsonSettings));
            return code;
        }
        internal override bool Exists(string key) => Cache.KeyExists(CloudKeyToString(key));
        internal override bool Delete(string key) => Cache.KeyDelete(CloudKeyToString(key));
        internal override IEnumerable<string> List()
        {
            string toReplace = $"{FullName}{MultitonConst.Separator}";
            List<string> keys = new List<string>();
            foreach (string redisKey in Cache.Multiplexer.GetServer(Cache.Multiplexer.GetEndPoints().First()).Keys())
                if (redisKey.Contains(toReplace))
                    keys.Add(redisKey.Replace(toReplace, string.Empty));
            return keys;
        }
        private static string CloudKeyToString(string keyString)
           => $"{FullName}{MultitonConst.Separator}{keyString}";
    }
}
