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
        private readonly static Dictionary<string, ValueMutltitonProperties> KeyContexts = new Dictionary<string, ValueMutltitonProperties>();
        private class ValueMutltitonProperties
        {
            internal Type ValueType { get; set; }
            internal MultitonProperties MultitonProperties { get; set; }
        }
        private static void Configure(Type keyType, Type type, MultitonProperties multitonProperties)
        {
            if (!KeyContexts.ContainsKey(keyType.FullName))
                KeyContexts.Add(keyType.FullName, new ValueMutltitonProperties() { ValueType = type, MultitonProperties = multitonProperties });
            else
                throw new ArgumentException($"Key {keyType.FullName} already installed for instace {KeyContexts[keyType.FullName].ValueType.FullName}");
        }

        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Cache o TableStorage connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update  Cache (default: 0, infinite), TableStorage has only infinite value</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only  cache) (with 0 you can use a Multiton without update time)</param>
        public static void Configure<TKey, TEntry>(MultitonProperties properties)
            where TKey : IMultiKey, new()
            where TEntry : IMultiton, new()
        {
            Configure(typeof(TKey), typeof(TEntry), properties);
        }
        public static MultitonProperties GetConfiguration(Type keyType)
        {
            if (KeyContexts.ContainsKey(keyType.FullName))
                return KeyContexts[keyType.FullName].MultitonProperties;
            throw new NotImplementedException($"{keyType.FullName} didn't installed. Please use MultitonInstaller.Configure in static constructor of your key class to set ConnectionString and parameters of caching and heap multiton.");
        }
        public static Type GetKeyType(Type keyType)
        {
            if (KeyContexts.ContainsKey(keyType.FullName))
                return KeyContexts[keyType.FullName].ValueType;
            throw new NotImplementedException($"{keyType.FullName} didn't installed. Please use MultitonInstaller.Configure in static constructor of your key class to set ConnectionString and parameters of caching and heap multiton.");
        }
    }
}
