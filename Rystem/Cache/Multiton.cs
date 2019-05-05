﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rystem.Cache
{
    internal delegate AMultiton CreationFunction(AMultitonKey key);
    public abstract class AMultiton
    {
        internal long LastUpdate = 0;
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        public abstract AMultiton Fetch(AMultitonKey key);
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        protected static void Install<TEntity>(string connectionString, CacheExpireTime expireCache = CacheExpireTime.Infinite, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TEntity : AMultiton
        {
            MultitonManager<TEntity>.OnStart(connectionString, (int)expireCache, (int)expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        protected static void Install<TEntity>(string connectionString, int expireCache, int expireMultiton)
            where TEntity : AMultiton
        {
            MultitonManager<TEntity>.OnStart(connectionString, (int)expireCache, (int)expireMultiton);
        }
    }
    internal partial class MultitonManager<TEntry> where TEntry : AMultiton
    {
        private static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private static object TrafficLightMethod = new object();
        private static MethodInfo MethodInstance(MethodType methodType = MethodType.Instance)
        {
            Type type = typeof(TEntry);
            string key = $"{methodType}_{type.FullName}";
            if (!methods.ContainsKey(key))
            {
                lock (TrafficLightMethod)
                {
                    if (!methods.ContainsKey(key))
                    {
                        switch (methodType)
                        {
                            case MethodType.Instance:
                                methods.Add(key, (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Instance", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Delete:
                                methods.Add(key, (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Delete", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Update:
                                methods.Add(key, (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Update", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Exists:
                                methods.Add(key, (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Exists", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.List:
                                methods.Add(key, (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("List", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                        }
                    }
                }
            }
            return methods[key];
        }
    }
    internal partial class MultitonManager<TEntry> where TEntry : AMultiton
    {
        private static Dictionary<string, TEntry> multitonDictionary = new Dictionary<string, TEntry>();
        private static object TrafficLight = new object();
        private static int ExpireMultiton = 0;
        private static bool HasCache = false;
        private static bool HasTableStorage = false;
        private static CreationFunction creationFunction = ((AMultiton)Activator.CreateInstance(typeof(TEntry))).Fetch;
        static MultitonManager()
        {
            Type type = typeof(TEntry);
            Activator.CreateInstance(type);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Redis Cache connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update Redis Cache (default: 0, infinite)</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only redis cache) (with 0 you can use a Multiton without update time)</param>
        internal static void OnStart(string connectionString, int expireCache = 0, int expireMultiton = -1)
        {
            ExpireMultiton = expireMultiton;
            HasCache = !string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains(".redis.cache.windows.net");
            HasTableStorage = !string.IsNullOrWhiteSpace(connectionString) && !HasCache;
            if (HasCache)
            {
                MultitonCache<TEntry>.OnStart(connectionString, expireCache);
                HasCache = true;
            }
            else if (HasTableStorage)
            {
                MultitonTableStorage<TEntry>.OnStart(connectionString, expireCache);
            }
        }
        /// <summary>
        /// Retrieve value of Instance
        /// </summary>
        /// <param name="key">instance key, the real key is composed from typeof(Class of Key).FullName and key.ToString()</param>
        /// <returns></returns>
        private static TEntry Instance(AMultitonKey key)
        {
            Type type = typeof(TEntry);
            string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                {
                    lock (TrafficLight)
                    {
                        if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                        {
                            if (!multitonDictionary.ContainsKey(innerKey)) multitonDictionary.Add(innerKey, null);
                            if (HasCache || HasTableStorage)
                            {
                                multitonDictionary[innerKey] = (TEntry)MethodInstance(MethodType.Instance).Invoke(null, new object[2] { key, creationFunction });
                                if (ExpireMultiton > 0 && multitonDictionary[innerKey] != null) multitonDictionary[innerKey].LastUpdate = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks;
                            }
                            else
                            {
                                multitonDictionary[innerKey] = (TEntry)((AMultiton)Activator.CreateInstance(type)).Fetch(key);
                                if (ExpireMultiton > 0 && multitonDictionary[innerKey] != null) multitonDictionary[innerKey].LastUpdate = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks;
                            }
                        }
                    }
                }
                return multitonDictionary[innerKey];
            }
            else
            {
                if (HasCache || HasTableStorage)
                {
                    return (TEntry)MethodInstance(MethodType.Instance).Invoke(null, new object[2] { key, creationFunction });
                }
                else
                {
                    return (TEntry)((AMultiton)Activator.CreateInstance(type)).Fetch(key);
                }
            }
        }
        private static bool Update(AMultitonKey key, object value = null)
        {
            bool updated = false;
            Type type = typeof(TEntry);
            if (value == null) value = ((AMultiton)Activator.CreateInstance(type)).Fetch(key);
            if (value == null) return updated;
            if (HasCache || HasTableStorage)
                updated = (bool)MethodInstance(MethodType.Update).Invoke(null, new object[2] { key, value });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
                if (!multitonDictionary.ContainsKey(innerKey))
                    multitonDictionary.Add(innerKey, (TEntry)value);
                else
                    multitonDictionary[innerKey] = (TEntry)value;
                updated &= true;
            }
            return updated;
        }
        private static bool Exists(AMultitonKey key)
        {
            bool existed = false;
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
                existed = (bool)MethodInstance(MethodType.Exists).Invoke(null, new object[2] { key, type });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
                existed &= multitonDictionary.ContainsKey($"{type.FullName}{MultitonConst.Separator}{key.Value}");
            return existed;
        }
        private static bool Delete(AMultitonKey key)
        {
            bool deleted = false;
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
                deleted = (bool)MethodInstance(MethodType.Delete).Invoke(null, new object[2] { key, type });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
                if (multitonDictionary.ContainsKey(innerKey))
                    deleted &= multitonDictionary.Remove(innerKey);
            }
            return deleted;
        }
        private static List<TKey> List<TKey>() where TKey : AMultitonKey
        {
            Type type = typeof(TEntry);
            Type keyType = typeof(TKey);
            if (HasCache || HasTableStorage)
            {
                IEnumerable<string> listedKeys = (IEnumerable<string>)MethodInstance(MethodType.List).Invoke(null, new object[1] { type });
                List<TKey> keys = new List<TKey>();
                foreach (string key in listedKeys)
                {
                    TKey multitonKey = (TKey)Activator.CreateInstance(keyType);
                    IEnumerator<string> keyValues = PropertyValue(key);
                    if (!keyValues.MoveNext()) continue;
                    foreach (PropertyInfo property in MultitonConst.PropertyInfoDictionary[keyType])
                    {
                        property.SetValue(multitonKey, Convert.ChangeType(keyValues.Current, property.PropertyType));
                        if (!keyValues.MoveNext()) break;
                    }
                    keys.Add(multitonKey);
                }
                return keys;
            }
            else
                throw new NotImplementedException();
            //todo: Alessandro Rapiti - da implementare anche la ricerca su lista per quando si ha solo multiton
            IEnumerator<string> PropertyValue(string key)
            {
                foreach (string s in key.Split(MultitonConst.Separator))
                    yield return s;
            }
        }
    }
}
