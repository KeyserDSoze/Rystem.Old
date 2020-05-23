using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static partial class MultitonKeyExtensionMethod
    {
        internal static string ToKeyString<TCache>(this ICacheKey<TCache> multitonKey)
            => multitonKey.Value().Trim(MultitonConst.Separator);
        private static string Value<TCache>(this ICacheKey<TCache> multitonKey)
        {
            Type keyType = multitonKey.GetType();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (PropertyInfo propertyInfo in MultitonConst.Instance(keyType))
                valueBuilder.Append($"{MultitonConst.Separator}{propertyInfo.GetValue(multitonKey)}");
            return valueBuilder.ToString();
        }
        private static readonly object TrafficLight = new object();
        private static readonly Dictionary<string, ICacheManager> CacheManagers = new Dictionary<string, ICacheManager>();
        private static ICacheManager<TCacheKey, TCache> Manager<TCacheKey, TCache>(this TCacheKey key)
            where TCacheKey : ICacheKey<TCache>
        {
            string installingKeyValue = $"{key.GetType().FullName}{typeof(TCache).FullName}";
            if (!CacheManagers.ContainsKey(installingKeyValue))
                lock (TrafficLight)
                    if (!CacheManagers.ContainsKey(installingKeyValue))
                        CacheManagers.Add(installingKeyValue, new CacheManager<TCacheKey, TCache>(key.CacheBuilder()));
            return CacheManagers[installingKeyValue] as CacheManager<TCacheKey, TCache>;
        }

        public static async Task<TEntry> InstanceAsync<TEntry>(this ICacheKey<TEntry> entry)
            => await entry.Manager<ICacheKey<TEntry>, TEntry>().InstanceAsync(entry).NoContext();
        public static async Task<bool> RemoveAsync<TEntry>(this ICacheKey<TEntry> entry)
            => await entry.Manager<ICacheKey<TEntry>, TEntry>().DeleteAsync(entry).NoContext();
        public static async Task<bool> RestoreAsync<TEntry>(this ICacheKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
           => await entry.Manager<ICacheKey<TEntry>, TEntry>().UpdateAsync(entry, value, expiringTime).NoContext();
        public static async Task<bool> IsPresentAsync<TEntry>(this ICacheKey<TEntry> entry)
           => await entry.Manager<ICacheKey<TEntry>, TEntry>().ExistsAsync(entry).NoContext();
        public static async Task WarmUpAsync<TEntry>(this ICacheKey<TEntry> entry)
           => await entry.Manager<ICacheKey<TEntry>, TEntry>().WarmUp().NoContext();
        public static async Task<IList<ICacheKey<TEntry>>> KeysAsync<TEntry>(this ICacheKey<TEntry> entry)
        {
            Type keyType = entry.GetType();
            IList<ICacheKey<TEntry>> keys = new List<ICacheKey<TEntry>>();
            foreach (string key in await entry.Manager<ICacheKey<TEntry>, TEntry>().ListAsync())
            {
                ICacheKey<TEntry> multitonKey = (ICacheKey<TEntry>)Activator.CreateInstance(keyType);
                IEnumerator<string> keyValues = PropertyValue(key);
                if (!keyValues.MoveNext())
                    continue;
                foreach (PropertyInfo property in MultitonConst.Instance(keyType))
                {
                    property.SetValue(multitonKey, Convert.ChangeType(keyValues.Current, property.PropertyType));
                    if (!keyValues.MoveNext())
                        break;
                }
                keys.Add(multitonKey);
            }
            return keys;
            IEnumerator<string> PropertyValue(string key)
            {
                foreach (string s in key.Split(MultitonConst.Separator))
                    yield return s;
            }
        }

        public static TEntry Instance<TEntry>(this ICacheKey<TEntry> entry)
            => entry.InstanceAsync().ToResult();
        public static bool Remove<TEntry>(this ICacheKey<TEntry> entry)
            => entry.RemoveAsync().ToResult();
        public static bool Restore<TEntry>(this ICacheKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
           => entry.RestoreAsync(value, expiringTime).ToResult();
        public static bool IsPresent<TEntry>(this ICacheKey<TEntry> entry)
            => entry.IsPresentAsync().ToResult();
        public static void WarmUp<TEntry>(this ICacheKey<TEntry> entry)
           => entry.WarmUpAsync().ToResult();
        public static IList<ICacheKey<TEntry>> Keys<TEntry>(this ICacheKey<TEntry> entry)
            => entry.KeysAsync().ToResult();
    }
}
