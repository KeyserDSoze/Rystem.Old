using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public static class MultitonKeyExtensionMethod
    {
        public static string ToKeyString(this IMultitonKey multitonKey) => multitonKey.Value().Trim(MultitonConst.Separator);
        private static string Value(this IMultitonKey multitonKey)
        {
            Type keyType = multitonKey.GetType();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (PropertyInfo propertyInfo in MultitonConst.Instance(keyType))
                valueBuilder.Append($"{MultitonConst.Separator}{propertyInfo.GetValue(multitonKey)}");
            return valueBuilder.ToString();
        }
        private static Dictionary<string, AMultitonManager> Managers = new Dictionary<string, AMultitonManager>();
        private readonly static object TrafficLight = new object();
        private static AMultitonManager Manager(Type keyType)
        {
            if (!Managers.ContainsKey(keyType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(keyType.FullName))
                    {
                        Type valueType = MultitonInstaller.GetKeyType(keyType);
                        Type genericType = typeof(MultitonManager<>).MakeGenericType(valueType);
                        Managers.Add(keyType.FullName, (AMultitonManager)Activator.CreateInstance(genericType));
                    }
            return Managers[keyType.FullName];
        }
        public static dynamic Instance<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey
        {
            return Manager(entry.GetType()).Get(entry);
        }
        public static bool Remove<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey
        {
            return Manager(entry.GetType()).Delete(entry);
        }
        public static bool Restore<TEntry>(this TEntry entry, IMultiton value = null)
            where TEntry : IMultitonKey
        {
            return Manager(entry.GetType()).Update(entry, value);
        }
        public static bool IsPresent<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey
        {
            return Manager(entry.GetType()).Exists(entry);
        }
        public static List<TEntry> AllKeys<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey, new()
        {
            Type keyType = typeof(TEntry);
            List<TEntry> keys = new List<TEntry>();
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
