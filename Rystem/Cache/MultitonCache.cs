﻿using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.Cache
{
    internal class MultitonCache<TEntry> where TEntry : AMultiton
    {
        private static string ConnectionString;
        private static IDatabase Cache
        {
            get
            {
                return Connection.Value.GetDatabase();
            }
        }
        private static Lazy<ConnectionMultiplexer> Connection;
        private static readonly object TrafficLight = new object();
        private static readonly object OnStartTrafficLight = new object();
        private static int ExpireCache = 0;

        internal static void OnStart(string connectionString, int expireCache = 0)
        {
            ConnectionString = connectionString;
            ExpireCache = expireCache;
            if (Connection == null)
            {
                lock (OnStartTrafficLight)
                {
                    if (Connection == null)
                    {
                        Connection = new Lazy<ConnectionMultiplexer>(() =>
                        {
                            string cacheConnection = ConnectionString;
                            return ConnectionMultiplexer.Connect(ConnectionString);
                        });
                    }
                }
            }
        }
        internal static TEntry Instance(AMultitonKey key, CreationFunction functionIfNotExists)
        {
            string keyString = $"{typeof(TEntry).FullName}{MultitonConst.Separator}{key.Value}";
            if (!Cache.KeyExists(keyString))
            {
                lock (TrafficLight)
                {
                    if (!Cache.KeyExists(keyString))
                    {
                        TEntry query = (TEntry)functionIfNotExists(key);
                        if (query != null)
                        {
                            if (ExpireCache > 0)
                            {
                                Cache.StringSet(keyString, JsonConvert.SerializeObject(query, MultitonConst.JsonSettings), TimeSpan.FromMinutes(ExpireCache));
                            }
                            else
                            {
                                Cache.StringSet(keyString, JsonConvert.SerializeObject(query, MultitonConst.JsonSettings));
                            }
                            return query;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return JsonConvert.DeserializeObject<TEntry>(Cache.StringGet(keyString), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        internal static bool Update(AMultitonKey key, TEntry value)
        {
            string keyString = $"{typeof(TEntry).FullName}{MultitonConst.Separator}{key.Value}";
            bool code = false;
            if (ExpireCache > 0)
            {
                code = Cache.StringSet(keyString, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings), TimeSpan.FromMinutes(ExpireCache));
            }
            else
            {
                code = Cache.StringSet(keyString, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings));
            }
            return code;
        }
        internal static bool Exists(AMultitonKey key, Type type)
        {
            string keyString = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
            return Cache.KeyExists(keyString);
        }
        internal static bool Delete(AMultitonKey key, Type type)
        {
            string keyString = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
            return Cache.KeyDelete(keyString);
        }
        internal static IEnumerable<string> List(Type type)
        {
            string toReplace = $"{type.FullName}{MultitonConst.Separator}";
            List<string> keys = new List<string>();
            foreach (string redisKey in Cache.Multiplexer.GetServer(Cache.Multiplexer.GetEndPoints().First()).Keys())
                if (redisKey.Contains(toReplace))
                    keys.Add(redisKey.Replace(toReplace, string.Empty));
            return keys;

        }
    }
}
