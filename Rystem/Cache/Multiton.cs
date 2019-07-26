using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rystem.Cache
{
    internal delegate IMultiton CreationFunction(IMultitonKey key);
    internal partial class MultitonManager<TEntry> where TEntry : IMultiton
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
    internal partial class MultitonManager<TEntry> where TEntry : IMultiton
    {
        private class MultitonDummy
        {
            internal long LastUpdate { get; set; } = 0;
            internal TEntry Entry { get; set; }
            public static implicit operator TEntry(MultitonDummy multitonDummy)
            {
                return multitonDummy.Entry;
            }
            public static implicit operator MultitonDummy(TEntry entry)
            {
                return new MultitonDummy()
                {
                    Entry = entry,
                    LastUpdate = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks
                };
            }
        }
        private static Dictionary<string, MultitonDummy> multitonDictionary = new Dictionary<string, MultitonDummy>();
        private static object TrafficLight = new object();
        private static int ExpireMultiton = 0;
        private static bool HasCache = false;
        private static bool HasTableStorage = false;
        private static CreationFunction creationFunction = ((IMultiton)Activator.CreateInstance(typeof(TEntry))).Fetch;
        static MultitonManager()
        {
            Type type = typeof(TEntry);
            MultitonInstaller.MultitonConfiguration connectionMultiton = MultitonInstaller.GetConfiguration(type);
            ExpireMultiton = connectionMultiton.ExpireMultiton;
            HasCache = !string.IsNullOrWhiteSpace(connectionMultiton.ConnectionString) && connectionMultiton.ConnectionString.Contains(".redis.cache.windows.net");
            HasTableStorage = !string.IsNullOrWhiteSpace(connectionMultiton.ConnectionString) && !HasCache;
            if (HasCache)
            {
                MultitonCache<TEntry>.OnStart(connectionMultiton.ConnectionString, connectionMultiton.ExpireCache);
                HasCache = true;
            }
            else if (HasTableStorage)
            {
                MultitonTableStorage<TEntry>.OnStart(connectionMultiton.ConnectionString, connectionMultiton.ExpireCache);
            }
        }
        /// <summary>
        /// Retrieve value of Instance
        /// </summary>
        /// <param name="key">instance key, the real key is composed from typeof(Class of Key).FullName and key.ToString()</param>
        /// <returns></returns>
        private static TEntry Instance(IMultitonKey key)
        {
            Type type = typeof(TEntry);
            string innerKey = $"{type.FullName}{key.Value()}";
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                {
                    lock (TrafficLight)
                    {
                        if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                        {
                            if (!multitonDictionary.ContainsKey(innerKey))
                                multitonDictionary.Add(innerKey, null);
                            if (HasCache || HasTableStorage)
                                multitonDictionary[innerKey] = (TEntry)MethodInstance(MethodType.Instance).Invoke(null, new object[2] { key, creationFunction });
                            else
                                multitonDictionary[innerKey] = (TEntry)((IMultiton)Activator.CreateInstance(type)).Fetch(key);
                        }
                    }
                }
                return multitonDictionary[innerKey];
            }
            else
            {
                if (HasCache || HasTableStorage)
                    return (TEntry)MethodInstance(MethodType.Instance).Invoke(null, new object[2] { key, creationFunction });
                else
                    return (TEntry)((IMultiton)Activator.CreateInstance(type)).Fetch(key);
            }
        }
        private static bool Update(IMultitonKey key, object value = null)
        {
            bool updated = false;
            Type type = typeof(TEntry);
            if (value == null) value = ((IMultiton)Activator.CreateInstance(type)).Fetch(key);
            if (value == null) return updated;
            if (HasCache || HasTableStorage)
                updated = (bool)MethodInstance(MethodType.Update).Invoke(null, new object[2] { key, value });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                string innerKey = $"{type.FullName}{key.Value()}";
                if (!multitonDictionary.ContainsKey(innerKey))
                    multitonDictionary.Add(innerKey, (TEntry)value);
                else
                    multitonDictionary[innerKey] = (TEntry)value;
                updated &= true;
            }
            return updated;
        }
        private static bool Exists(IMultitonKey key)
        {
            bool existed = false;
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
                existed = (bool)MethodInstance(MethodType.Exists).Invoke(null, new object[2] { key, type });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
                existed &= multitonDictionary.ContainsKey($"{type.FullName}{key.Value()}");
            return existed;
        }
        private static bool Delete(IMultitonKey key)
        {
            bool deleted = false;
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
                deleted = (bool)MethodInstance(MethodType.Delete).Invoke(null, new object[2] { key, type });
            if (ExpireMultiton > (int)MultitonExpireTime.TurnOff)
            {
                string innerKey = $"{type.FullName}{key.Value()}";
                if (multitonDictionary.ContainsKey(innerKey))
                    deleted &= multitonDictionary.Remove(innerKey);
            }
            return deleted;
        }
        private static List<TKey> List<TKey>() where TKey : IMultitonKey
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
                    
                    foreach (PropertyInfo property in MultitonConst.Instance(keyType))
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
