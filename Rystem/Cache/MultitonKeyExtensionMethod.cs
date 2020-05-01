using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        private readonly static Dictionary<string, IMultitonManager> Managers = new Dictionary<string, IMultitonManager>();
        private readonly static object TrafficLight = new object();
        private static IMultitonManager Manager<TEntry>(Type keyType)
            where TEntry : IMultiton, new()
        {
            if (!Managers.ContainsKey(keyType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(keyType.FullName))
                        Managers.Add(keyType.FullName, new MultitonManager<TEntry>(MultitonInstaller.GetConfiguration(keyType)));
            return Managers[keyType.FullName];
        }
        public static TEntry Instance<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => Manager<TEntry>(entry.GetType()).Instance(entry);
        public static bool Remove<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => Manager<TEntry>(entry.GetType()).Delete(entry);
        public static bool Restore<TEntry>(this IMultitonKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
            where TEntry : IMultiton, new()
            => Manager<TEntry>(entry.GetType()).Update(entry, value, expiringTime);
        public static bool IsPresent<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => Manager<TEntry>(entry.GetType()).Exists(entry);
        public static IList<TKey> Keys<TKey, TEntry>(this IMultitonKey<TEntry> entry)
            where TKey : IMultitonKey<TEntry>, new()
            where TEntry : IMultiton, new()
        {
            Type keyType = entry.GetType();
            IList<TKey> keys = new List<TKey>();
            foreach (string key in Manager<TEntry>(keyType).List())
            {
                TKey multitonKey = new TKey();
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
    }
}
