using System;
using System.Collections.Generic;

namespace Rystem.Cache
{
    /// <summary>
    /// Install Multiton paradigma for your Entity.
    /// </summary>
    /// <typeparam name="TEntry">AMultiton Entity</typeparam>
    public static class MultitonInstaller
    {
        private static Dictionary<string, MultitonConfiguration> Contexts = new Dictionary<string, MultitonConfiguration>();
        private static Dictionary<string, MultitonConfiguration> KeyContexts = new Dictionary<string, MultitonConfiguration>();
        public class MultitonConfiguration
        {
            public string ConnectionString { get; set; }
            public int ExpireCache { get; set; }
            public int ExpireMultiton { get; set; }
            public Type KeyType { get; set; }
            public Type Type { get; set; }
            public InCloudType InCloudType { get; set; }
        }
        private static void Configure(Type keyType, Type type, InCloudType inCloudType, string connectionString, int expireCache = 0, int expireMultiton = -1)
        {
            if (!Contexts.ContainsKey(type.FullName) || !KeyContexts.ContainsKey(keyType.FullName))
            {
                MultitonConfiguration multitonConfiguration = new MultitonConfiguration()
                {
                    ConnectionString = connectionString,
                    ExpireCache = expireCache,
                    ExpireMultiton = expireMultiton,
                    KeyType = keyType,
                    Type = type,
                    InCloudType = inCloudType
                };
                if (!Contexts.ContainsKey(type.FullName))
                    Contexts.Add(type.FullName, multitonConfiguration);
                if (Contexts[type.FullName].KeyType.FullName != keyType.FullName)
                    throw new ArgumentException($"Too many keys found for {keyType.FullName}. A key named {Contexts[type.FullName].KeyType.FullName} already exists for instace {type.FullName}.");
                if (!KeyContexts.ContainsKey(keyType.FullName))
                    KeyContexts.Add(keyType.FullName, multitonConfiguration);
            }
        }

        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TKey, TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, CacheExpireTime expireCache = CacheExpireTime.Infinite, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TKey : IMultitonKey
            where TEntry : IMultiton
        {
            Configure(typeof(TKey), typeof(TEntry), inCloudType, connectionString, (int)expireCache, (int)expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TKey, TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, int expireCache = 0, int expireMultiton = -1)
             where TKey : IMultitonKey
             where TEntry : IMultiton
        {
            Configure(typeof(TKey), typeof(TEntry), inCloudType, connectionString, expireCache, expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TKey, TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, int expireCache = 0, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TKey : IMultitonKey
            where TEntry : IMultiton
        {
            Configure(typeof(TKey), typeof(TEntry), inCloudType, connectionString, expireCache, (int)expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TKey, TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, CacheExpireTime expireCache = CacheExpireTime.Infinite, int expireMultiton = -1)
            where TKey : IMultitonKey
            where TEntry : IMultiton
        {
            Configure(typeof(TKey), typeof(TEntry), inCloudType, connectionString, (int)expireCache, expireMultiton);
        }
        public static void Configure<TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, CacheExpireTime expireCache = CacheExpireTime.Infinite, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TEntry : IMulti
        {
            Configure<TEntry>(connectionString, inCloudType, (int)expireCache, (int)expireMultiton);
        }
        public static void Configure<TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, int expireCache = 0, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
           where TEntry : IMulti
        {
            Configure<TEntry>(connectionString, inCloudType, (int)expireCache, (int)expireMultiton);
        }
        public static void Configure<TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, CacheExpireTime expireCache = CacheExpireTime.Infinite, int expireMultiton = -1)
           where TEntry : IMulti
        {
            Configure<TEntry>(connectionString, inCloudType, (int)expireCache, (int)expireMultiton);
        }
        public static void Configure<TEntry>(string connectionString, InCloudType inCloudType = InCloudType.RedisCache, int expireCache = 0, int expireMultiton = -1)
          where TEntry : IMulti
        {
            if (typeof(IMultiton).IsAssignableFrom(typeof(TEntry)))
            {
                Type type = typeof(TEntry);
                string result = ReplaceFirstOccurrence(type.AssemblyQualifiedName, ", ", "Key, ");
                Type keyType = Type.GetType(result);
                if (keyType == null)
                    throw new ArgumentException($"{type.FullName} doesn't exist its multiton key class. Please create yourClass and yourClassKey in the same namespace and same assembly *pay attention to call them ClassName and ClassNameKey.");
                Configure(keyType, type, inCloudType, connectionString, expireCache, expireMultiton);
            }
            else
            {
                Type keyType = typeof(TEntry);
                string result = ReplaceFirstOccurrence(keyType.AssemblyQualifiedName, "Key, ", ", ");
                Type type = Type.GetType(result);
                if (type == null)
                    throw new ArgumentException($"{type.FullName} doesn't exist its multiton class. Please create yourClass and yourClassKey in the same namespace and same assembly *pay attention to call them ClassName and ClassNameKey.");
                Configure(keyType, type, inCloudType, connectionString, expireCache, expireMultiton);
            }
        }
        private static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }
        public static MultitonConfiguration GetConfiguration(Type type)
        {
            if (Contexts.ContainsKey(type.FullName))
                return Contexts[type.FullName];
            throw new NotImplementedException("Please use Install static method in static constructor of your key class to set ConnectionString and parameters of caching and heap multiton.");
        }
        public static Type GetKeyType(Type keyType) => KeyContexts[keyType.FullName].Type;
    }
    /// <summary>
    /// Cache Expire Time Enumerator
    /// </summary>
    public enum CacheExpireTime
    {
        /// <summary>
        /// Data isn't stored in cache
        /// </summary>
        TurnOff = -1,
        /// <summary>
        /// Data stored in  cache always persists
        /// </summary>
        Infinite = 0,
        /// <summary>
        /// Data is stored in cache for 5 minutes
        /// </summary>
        FiveMinutes = 5,
        /// <summary>
        /// Data is stored in cache for 10 minutes
        /// </summary>
        TenMinutes = 10,
        /// <summary>
        /// Data is stored in cache for 1 hour
        /// </summary>
        OneHour = 60,
        /// <summary>
        /// Data is stored in cache for 8 hours
        /// </summary>
        EightHour = 60 * 8,
        /// <summary>
        /// Data is stored in cache for 1 day
        /// </summary>
        OneDay = 60 * 24,
        /// <summary>
        /// Data is stored in cache for 1 week
        /// </summary>
        OneWeek = 60 * 24 * 7,
        /// <summary>
        /// Data is stored in cache for 1 month
        /// </summary>
        OneMonth = 60 * 24 * 7 * 30,
        /// <summary>
        /// Data is stored in cache for 6 months
        /// </summary>
        SixMonths = 60 * 24 * 7 * 30 * 6,
        /// <summary>
        /// Data is stored in cache for 360 days
        /// </summary>
        OneYear = 60 * 24 * 7 * 30 * 360
    }
    /// <summary>
    /// Multiton Expire Time Enumerator
    /// </summary>
    public enum MultitonExpireTime
    {
        /// <summary>
        /// Data isn't stored in memory app
        /// </summary>
        TurnOff = -1,
        /// <summary>
        /// Data stored in memory app always persists
        /// </summary>
        Infinite = 0,
        /// <summary>
        /// Data is stored in memory app for 5 minutes
        /// </summary>
        FiveMinutes = 5,
        /// <summary>
        /// Data is stored in memory app for 10 minutes
        /// </summary>
        TenMinutes = 10,
        /// <summary>
        /// Data is stored in memory app for 1 hour
        /// </summary>
        OneHour = 60,
        /// <summary>
        /// Data is stored in memory app for 8 hour
        /// </summary>
        EightHour = 60 * 8,
        /// <summary>
        /// Data is stored in memory app for 1 day
        /// </summary>
        OneDay = 60 * 24,
        /// <summary>
        /// Data is stored in memory app for 1 week
        /// </summary>
        OneWeek = 60 * 24 * 7,
        /// <summary>
        /// Data is stored in memory app for 30 days
        /// </summary>
        OneMonth = 60 * 24 * 7 * 30,
        /// <summary>
        /// Data is stored in cache for 6 months
        /// </summary>
        SixMonths = 60 * 24 * 7 * 30 * 6,
        /// <summary>
        /// Data is stored in memory app for 360 days
        /// </summary>
        OneYear = 60 * 24 * 7 * 30 * 360
    }
    public enum InCloudType
    {
        RedisCache,
        TableStorage,
        BlobStorage
    }
}
