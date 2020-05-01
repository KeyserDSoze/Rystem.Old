using Newtonsoft.Json;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.Cache
{
    internal class MultitonConst
    {
        private static Dictionary<string, IEnumerable<PropertyInfo>> PropertyInfoDictionary = new Dictionary<string, IEnumerable<PropertyInfo>>();
        private static object TrafficLight = new object();
        internal static IEnumerable<PropertyInfo> Instance(Type keyType)
        {
            if (!PropertyInfoDictionary.ContainsKey(keyType.FullName))
            {
                lock (TrafficLight)
                {
                    if (!PropertyInfoDictionary.ContainsKey(keyType.FullName))
                    {
                        PropertyInfoDictionary.Add(keyType.FullName, keyType.GetProperties().ToList().FindAll(x =>
                                    x.GetCustomAttribute(NoKey) == null && StringablePrimitive.Check(x.PropertyType)));
                        if (PropertyInfoDictionary[keyType.FullName].Count() == 0)
                            throw new ArgumentException($"{keyType.FullName} doesn't implement any primitive property to create the key as string.");
                    }
                }
            }
            return PropertyInfoDictionary[keyType.FullName];
        }
        public const char Separator = '╬';
        public static readonly Type NoKey = typeof(NoMultitonKey);
    }
}
