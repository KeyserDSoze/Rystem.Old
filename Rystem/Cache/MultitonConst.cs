using Newtonsoft.Json;
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
                                    x.GetCustomAttribute(NoKey) == null && CheckPrimitiveList(x.PropertyType)));
                        if (PropertyInfoDictionary[keyType.FullName].Count() == 0)
                            throw new ArgumentException($"{keyType.FullName} doesn't implement any primitive property to create the key as string.");
                    }
                }
            }
            return PropertyInfoDictionary[keyType.FullName];
        }
        private static bool CheckPrimitiveList(Type type)
        {
            foreach (Type typeR in MultitonConst.NormalTypes)
                if (typeR == type) return true;
            return false;
        }
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        public const char Separator = '╬';
        public static readonly Type NoKey = typeof(NoMultitonKey);
        public static readonly Type MultitonKey = typeof(IMultitonKey);
        public static readonly List<Type> NormalTypes = new List<Type>
        {
            typeof(int),
            typeof(bool),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(long),
            typeof(byte),
            typeof(sbyte),
            typeof(float),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
            typeof(int?),
            typeof(bool?),
            typeof(char?),
            typeof(decimal?),
            typeof(double?),
            typeof(long?),
            typeof(byte?),
            typeof(sbyte?),
            typeof(float?),
            typeof(uint?),
            typeof(ulong?),
            typeof(short?),
            typeof(ushort?),
            typeof(Guid)
        };
    }
}
