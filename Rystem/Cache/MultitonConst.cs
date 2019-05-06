using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.Cache
{
    internal class MultitonConst
    {
        public static Dictionary<Type, IEnumerable<PropertyInfo>> PropertyInfoDictionary = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        static MultitonConst()
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!assembly.FullName.ToLower().Contains("system") && !assembly.FullName.ToLower().Contains("microsoft"))
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.GetInterfaces().ToList().Find(x => x == MultitonKey) != null)
                            {
                                PropertyInfoDictionary.Add(type,
                                    type.GetProperties().ToList().FindAll(x =>
                                        x.GetCustomAttribute(NoKey) == null && CheckPrimitiveList(x.PropertyType)));
                            }
                        }
                    }
                }
                catch
                {
                }
            }
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
        public static Type NoKey = typeof(NoMultitonKey);
        public static Type MultitonKey = typeof(IMultitonKey);
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
            typeof(ushort?)
        };
    }
}
