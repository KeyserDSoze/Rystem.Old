using Rystem.Interfaces.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.Cache
{
    /// <summary>
    /// Install Multiton paradigma for your Entity.
    /// </summary>
    /// <typeparam name="TEntry">AMultiton Entity</typeparam>
    public static class MultitonInstaller
    {
        private static Dictionary<string, MultitonConfiguration> Contexts = new Dictionary<string, MultitonConfiguration>();
        private static Dictionary<string, Type> KeyTypes = new Dictionary<string, Type>();
        public class MultitonConfiguration
        {
            public string ConnectionString { get; set; }
            public int ExpireCache { get; set; }
            public int ExpireMultiton { get; set; }
        }

        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TEntry>(string connectionString, Type keyType = null, CacheExpireTime expireCache = CacheExpireTime.Infinite, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TEntry : IMultiton
        {
            Configure<TEntry>(connectionString, keyType, (int)expireCache, (int)expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TEntry>(string connectionString, Type keyType = null, int expireCache = 0, int expireMultiton = -1)
            where TEntry : IMultiton
        {
            Type type = typeof(TEntry);
            if (!Contexts.ContainsKey(type.FullName))
            {
                Contexts.Add(type.FullName, new MultitonConfiguration()
                {
                    ConnectionString = connectionString,
                    ExpireCache = expireCache,
                    ExpireMultiton = expireMultiton
                });
                KeyTypes.Add(keyType?.Name ?? $"{type.Name}Key", type);
            }
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TEntry>(string connectionString, Type keyType = null, int expireCache = 0, MultitonExpireTime expireMultiton = MultitonExpireTime.TurnOff)
            where TEntry : IMultiton
        {
            Configure<TEntry>(connectionString, keyType, expireCache, (int)expireMultiton);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TEntry>(string connectionString, Type keyType = null, CacheExpireTime expireCache = CacheExpireTime.Infinite, int expireMultiton = -1)
            where TEntry : IMultiton
        {
            Configure<TEntry>(connectionString, keyType, (int)expireCache, expireMultiton);
        }
        public static MultitonConfiguration GetConfiguration(Type type)
        {
            if (Contexts.ContainsKey(type.FullName))
                return Contexts[type.FullName];
            throw new NotImplementedException("Please use Install static method in static constructor of your class to set ConnectionString and parameters of caching and heap multiton.");
        }
        private static readonly object KeyTrafficLight = new object();
        public static Type GetKeyType(Type keyType)
        {
            if (!KeyTypes.ContainsKey(keyType.Name))
            {
                lock (KeyTrafficLight)
                {
                    if (!KeyTypes.ContainsKey(keyType.Name))
                    {
                        string result = ReplaceFirstOccurrence(keyType.AssemblyQualifiedName, "Key, ", ", ");
                        Type valueType = Type.GetType(result);
                        if (valueType == null)
                            ForceInstallation();
                        else
                            Activator.CreateInstance(valueType);
                        if (!KeyTypes.ContainsKey(keyType.Name))
                            throw new ArgumentException($"{keyType.FullName} doesn't exist its multiton class. Please call yourClass and yourClassKey in the same namespace and same assembly.");
                    }
                }
            }
            return KeyTypes[keyType.Name];
            string ReplaceFirstOccurrence(string Source, string Find, string Replace)
            {
                int Place = Source.IndexOf(Find);
                string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
                return result;
            }
        }
        private static void ForceInstallation()
        {
            foreach (Type type in Assembler.Types.ToList().FindAll(ø => ø.GetInterface("IMultiton") != null))
            {
                Activator.CreateInstance(type);
            }
        }
    }
    /// <summary>
    /// Cache Expire Time Enumerator
    /// </summary>
    public enum CacheExpireTime
    {
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
        /// Data is stored in memory app for 360 days
        /// </summary>
        OneYear = 60 * 24 * 7 * 30 * 360
    }
}
