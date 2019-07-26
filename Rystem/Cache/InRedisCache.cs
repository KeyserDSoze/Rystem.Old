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
        private static int ExpireCache = 0;
        private readonly static string FullName = typeof(T).FullName;
        internal InRedisCache(MultitonInstaller.MultitonConfiguration configuration)
        {
            ExpireCache = configuration.ExpireCache;
            Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration.ConnectionString));
        }
        internal override T Instance(string key)
        {
            string json = Cache.StringGet(key);
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        internal override bool Update(string key, T value)
        {
            bool code = false;
            if (ExpireCache > 0)
                code = Cache.StringSet(key, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings), TimeSpan.FromMinutes(ExpireCache));
            else
                code = Cache.StringSet(key, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings));
            return code;
        }
        internal override bool Exists(string key)
        {
            return Cache.KeyExists(key);
        }
        internal override bool Delete(string key)
        {
            return Cache.KeyDelete(key);
        }
        internal override IEnumerable<string> List()
        {
            string toReplace = $"{FullName}{MultitonConst.Separator}";
            List<string> keys = new List<string>();
            foreach (string redisKey in Cache.Multiplexer.GetServer(Cache.Multiplexer.GetEndPoints().First()).Keys())
                if (redisKey.Contains(toReplace))
                    keys.Add(redisKey.Replace(toReplace, string.Empty));
            return keys;
        }
    }
}
