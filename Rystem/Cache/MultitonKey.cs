using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rystem.Cache
{
    public static class MultitonKeyExtensionMethod
    {
        public static string ToKeyString(this IMultitonKey multitonKey) => multitonKey.Value().Trim(MultitonConst.Separator);
        internal static string Value(this IMultitonKey multitonKey)
        {
            Type keyType = multitonKey.GetType();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (PropertyInfo propertyInfo in MultitonConst.Instance(keyType))
                valueBuilder.Append($"{MultitonConst.Separator}{propertyInfo.GetValue(multitonKey)}");
            return valueBuilder.ToString();
        }
        private static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private static object TrafficLight = new object();
        private static MethodInfo MethodInstance<TEntry>(TEntry entry, MethodType methodType = MethodType.Instance)
             where TEntry : IMultitonKey
        {
            Type keyType = entry.GetType();
            Type type = MultitonInstaller.GetKeyType(keyType);
            string key = $"{methodType}_{type.FullName}";
            if (!methods.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!methods.ContainsKey(key))
                    {
                        switch (methodType)
                        {
                            case MethodType.Instance:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(type).GetMethod("Instance", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Delete:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(type).GetMethod("Delete", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Update:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(type).GetMethod("Update", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Exists:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(type).GetMethod("Exists", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.List:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(type).GetMethod("List", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(entry.GetType()));
                                break;
                        }
                    }
                }
            }
            return methods[key];
        }
        public static dynamic Instance<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey
        {
            try
            {
                return MethodInstance(entry).Invoke(null, new object[1] { entry });
            }
            catch (Exception er)
            {
                string sale = er.ToString();
                return null;
            }
        }
        public static bool Remove<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey
        {
            return (bool)MethodInstance(entry, MethodType.Delete).Invoke(null, new object[1] { entry });
        }
        public static bool Restore<TEntry>(this TEntry entry, object value = null)
            where TEntry : IMultitonKey
        {
            return (bool)MethodInstance(entry, MethodType.Update).Invoke(null, new object[2] { entry, value });
        }
        public static bool IsPresent<TEntry>(this TEntry entry, string value = null)
            where TEntry : IMultitonKey
        {
            return (bool)MethodInstance(entry, MethodType.Exists).Invoke(null, new object[1] { entry });
        }
        public static List<TEntry> AllKeys<TEntry>(this TEntry entry)
            where TEntry : IMultitonKey, new()
        {
            return (List<TEntry>)MethodInstance(entry, MethodType.List).Invoke(null, null);
        }
    }
}
