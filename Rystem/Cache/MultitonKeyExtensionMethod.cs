using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public static class MultitonKeyExtensionMethod
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
        private static Dictionary<string, IMultitonManager> Managers = new Dictionary<string, IMultitonManager>();
        private readonly static object TrafficLight = new object();
        private static IMultitonManager Manager(Type keyType)
        {
            if (!Managers.ContainsKey(keyType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(keyType.FullName))
                    {
                        Type valueType = MultitonInstaller.GetKeyType(keyType);
                        Type managerType = typeof(MultitonManager<>).MakeGenericType(valueType);
                        Managers.Add(keyType.FullName, (IMultitonManager)Activator.CreateInstance(managerType, new object[1] { MultitonInstaller.GetConfiguration(keyType) }));
                    }
            return Managers[keyType.FullName];
        }
        [Obsolete("This method will be removed in future version. Please use IMultitonKey<TCache> instead of IMultitonKey.")]
        public static IMultiton Instance(this IMultitonKey entry)
            => Manager(entry.GetType()).Instance(entry);
        public static TEntry Instance<TEntry>(this IMultitonKey<TEntry> entry)
            where TEntry : IMultiton, new()
            => Manager(entry.GetType()).Instance(entry);

        public static bool Remove(this IMultiKey entry)
            => Manager(entry.GetType()).Delete(entry);

        [Obsolete("This method will be removed in future version. Please use IMultitonKey<TCache> instead of IMultitonKey.")]
        public static bool Restore(this IMultitonKey entry, IMultiton value = null, TimeSpan expiringTime = default)
            => Manager(entry.GetType()).Update(entry, value, expiringTime);
        public static bool Restore<TEntry>(this IMultitonKey<TEntry> entry, TEntry value = default, TimeSpan expiringTime = default)
            where TEntry : IMultiton, new()
            => Manager(entry.GetType()).Update(entry, value, expiringTime);

        public static bool IsPresent(this IMultiKey entry)
            => Manager(entry.GetType()).Exists(entry);
        public static IList<TEntry> AllKeys<TEntry>(this TEntry entry)
            where TEntry : IMultiKey, new()
        {
            Type keyType = typeof(TEntry);
            IList<TEntry> keys = new List<TEntry>();
            foreach (string key in Manager(keyType).List())
            {
                TEntry multitonKey = (TEntry)Activator.CreateInstance(keyType);
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
