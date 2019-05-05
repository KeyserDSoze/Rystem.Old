using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rystem.Cache
{
    public class NoMultitonKey : Attribute { }
    public abstract class AMultitonKey
    {
        internal Type MultitonType;
        private string value;
        [NoMultitonKey]
        internal string Value
        {
            get
            {
                if (value != null) return value;
                StringBuilder valueBuilder = new StringBuilder();
                foreach (PropertyInfo propertyInfo in MultitonConst.PropertyInfoDictionary[this.GetType()])
                {
                    valueBuilder.Append($"{propertyInfo.GetValue(this)}{MultitonConst.Separator}");
                }
                return value = valueBuilder.ToString().Trim(MultitonConst.Separator);
            }
        }
        public AMultitonKey(Type multitonType) => this.MultitonType = multitonType;
    }

    public static class MultitonKeyExtensionMethod
    {
        private static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private static object TrafficLight = new object();
        private static MethodInfo MethodInstance<TEntry>(TEntry entry, MethodType methodType = MethodType.Instance)
             where TEntry : AMultitonKey, new()
        {
            string key = $"{methodType}_{entry.MultitonType.FullName}";
            if (!methods.ContainsKey(key))
            {
                lock (TrafficLight)
                {
                    if (!methods.ContainsKey(key))
                    {
                        switch (methodType)
                        {
                            case MethodType.Instance:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(entry.MultitonType).GetMethod("Instance", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Delete:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(entry.MultitonType).GetMethod("Delete", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Update:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(entry.MultitonType).GetMethod("Update", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.Exists:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(entry.MultitonType).GetMethod("Exists", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic));
                                break;
                            case MethodType.List:
                                methods.Add(key, typeof(MultitonManager<>).MakeGenericType(entry.MultitonType).GetMethod("List", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(entry.GetType()));
                                break;
                        }
                    }
                }
            }
            return methods[key];
        }
        public static dynamic Instance<TEntry>(this TEntry entry)
            where TEntry : AMultitonKey, new()
        {
            return MethodInstance(entry).Invoke(null, new object[1] { entry });
        }
        public static bool Delete<TEntry>(this TEntry entry)
            where TEntry : AMultitonKey, new()
        {
            return (bool)MethodInstance(entry, MethodType.Delete).Invoke(null, new object[1] { entry });
        }
        public static bool Update<TEntry>(this TEntry entry, object value = null)
            where TEntry : AMultitonKey, new()
        {
            return (bool)MethodInstance(entry, MethodType.Update).Invoke(null, new object[2] { entry, value });
        }
        public static bool Exists<TEntry>(this TEntry entry, string value = null)
            where TEntry : AMultitonKey, new()
        {
            return (bool)MethodInstance(entry, MethodType.Exists).Invoke(null, new object[1] { entry });
        }
        public static List<TEntry> List<TEntry>(this TEntry entry)
            where TEntry : AMultitonKey, new()
        {
            return (List<TEntry>)MethodInstance(entry, MethodType.List).Invoke(null, null);
        }
    }
}
