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
        public static string ToKeyString(this IMultiKey multitonKey)
            => multitonKey.Value().Trim(MultitonConst.Separator);
        private static string Value(this IMultiKey multitonKey)
        {
            Type keyType = multitonKey.GetType();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (PropertyInfo propertyInfo in MultitonConst.Instance(keyType))
                valueBuilder.Append($"{MultitonConst.Separator}{propertyInfo.GetValue(multitonKey)}");
            return valueBuilder.ToString();
        }
        private class ManagerContainer<TEntry>
             where TEntry : IMultiton, new()
        {
            public readonly static Dictionary<string, IMultitonManager<TEntry>> Managers = new Dictionary<string, IMultitonManager<TEntry>>();
            public readonly static object TrafficLight = new object();
        }
        private static IMultitonManager<TEntry> Manager<TEntry>(Type keyType)
            where TEntry : IMultiton, new()
        {
            if (!ManagerContainer<TEntry>.Managers.ContainsKey(keyType.FullName))
                lock (ManagerContainer<TEntry>.TrafficLight)
                    if (!ManagerContainer<TEntry>.Managers.ContainsKey(keyType.FullName))
                        ManagerContainer<TEntry>.Managers.Add(keyType.FullName, new MultitonManager<TEntry>(MultitonInstaller.GetConfiguration(keyType)));
            return ManagerContainer<TEntry>.Managers[keyType.FullName];
        }

        public static async Task<TEntry> InstanceAsync<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => await Manager<TEntry>(entry.GetType()).InstanceAsync(entry).NoContext();
        public static async Task<bool> RemoveAsync<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => await Manager<TEntry>(entry.GetType()).DeleteAsync(entry).NoContext();
        public static async Task<bool> RestoreAsync<TEntry>(this IMultitonKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
           where TEntry : IMultiton, new()
           => await Manager<TEntry>(entry.GetType()).UpdateAsync(entry, value, expiringTime).NoContext();
        public static async Task<bool> IsPresentAsync<TEntry>(this IMultitonKey<TEntry> entry)
           where TEntry : IMultiton, new()
           => await Manager<TEntry>(entry.GetType()).ExistsAsync(entry).NoContext();
        public static async Task WarmUpAsync<TEntry>(this IMultitonKey<TEntry> entry)
           where TEntry : IMultiton, new()
           => await Manager<TEntry>(entry.GetType()).WarmUp().NoContext();
        public static async Task<IList<IMultitonKey<TEntry>>> KeysAsync<TEntry>(this IMultitonKey<TEntry> entry)
           where TEntry : IMultiton, new()
        {
            Type keyType = entry.GetType();
            IList<IMultitonKey<TEntry>> keys = new List<IMultitonKey<TEntry>>();
            foreach (string key in await Manager<TEntry>(keyType).ListAsync())
            {
                IMultitonKey<TEntry> multitonKey = (IMultitonKey<TEntry>)Activator.CreateInstance(keyType);
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

        public static TEntry Instance<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => entry.InstanceAsync().ToResult();
        public static bool Remove<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => entry.RemoveAsync().ToResult();
        public static bool Restore<TEntry>(this IMultitonKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
           where TEntry : IMultiton, new()
           => entry.RestoreAsync(value, expiringTime).ToResult();
        public static bool IsPresent<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => entry.IsPresentAsync().ToResult();
        public static void WarmUp<TEntry>(this IMultitonKey<TEntry> entry)
           where TEntry : IMultiton, new()
           => entry.WarmUpAsync().ToResult();
        public static IList<IMultitonKey<TEntry>> Keys<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => entry.KeysAsync().ToResult();
    }
}
