using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal delegate AMultiton CreationFunction(AMultitonKey key);
    public abstract class AMultiton
    {
        internal long LastUpdate = 0;
        /// <summary>
        /// Fetch data of the istance by your database, or webrequest, or your business logic.
        /// </summary>
        /// <param name="key">Your istance Id.</param>
        /// <returns>This istance.</returns>
        public abstract AMultiton Fetch(AMultitonKey key);
    }
    public abstract class AMultitonKey
    {

        private string value;
        [NoMultitonKey]
        public string Value
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
    }
    public partial class MultitonManager<TEntry> where TEntry : AMultiton
    {
        private static Dictionary<string, TEntry> multitonDictionary = new Dictionary<string, TEntry>();
        private static object TrafficLight = new object();
        private static int ExpireMultiton = 0;
        private static bool HasCache = false;
        private static bool HasTableStorage = false;
        static MultitonManager()
        {
            Type type = typeof(TEntry);
            Activator.CreateInstance(type);
        }
        /// <summary>
        /// Call on start of your application.
        /// </summary>
        /// <param name="connectionString">Redis Cache connectionstring (default: null [no cache used])</param>
        /// <param name="expireCache">timespan for next update Redis Cache (default: 0, infinite)</param>
        /// <param name="expireMultiton">timespan for next update Multiton (default: -1, turn off, use only redis cache) (with 0 you can use a Multiton without update time)</param>
        internal static void OnStart(string connectionString, int expireCache = 0, int expireMultiton = -1)
        {
            ExpireMultiton = expireMultiton;
            HasCache = !string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains(".redis.cache.windows.net");
            HasTableStorage = !string.IsNullOrWhiteSpace(connectionString) && !HasCache;
            if (HasCache)
            {
                MultitonCache<TEntry>.OnStart(connectionString, expireCache);
                HasCache = true;
            }
            else if (HasTableStorage)
            {
                MultitonTableStorage<TEntry>.OnStart(connectionString, expireCache);
            }
        }
        /// <summary>
        /// Retrieve value of Instance
        /// </summary>
        /// <param name="key">instance key, the real key is composed from typeof(Class of Key).FullName and key.ToString()</param>
        /// <returns></returns>
        public static TEntry Instance(AMultitonKey key)
        {
            Type type = typeof(TEntry);
            string nameInstance = type.FullName;
            string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
            if (ExpireMultiton > -1)
            {
                if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                {
                    lock (TrafficLight)
                    {
                        if (!multitonDictionary.ContainsKey(innerKey) || (ExpireMultiton > 0 && multitonDictionary[innerKey]?.LastUpdate < DateTime.UtcNow.Ticks) || multitonDictionary[innerKey] == null)
                        {
                            if (!multitonDictionary.ContainsKey(innerKey)) multitonDictionary.Add(innerKey, null);
                            if (HasCache || HasTableStorage)
                            {
                                multitonDictionary[innerKey] = (TEntry)FromCache(key, type);
                                if (ExpireMultiton > 0 && multitonDictionary[innerKey] != null) multitonDictionary[innerKey].LastUpdate = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks;
                            }
                            else
                            {
                                multitonDictionary[innerKey] = (TEntry)((AMultiton)Activator.CreateInstance(type)).Fetch(key);
                                if (ExpireMultiton > 0 && multitonDictionary[innerKey] != null) multitonDictionary[innerKey].LastUpdate = DateTime.UtcNow.AddMinutes(ExpireMultiton).Ticks;
                            }
                        }
                    }
                }
                return (TEntry)multitonDictionary[innerKey];
            }
            else
            {
                if (HasCache || HasTableStorage)
                {
                    return (TEntry)FromCache(key, type);
                }
                else
                {
                    return (TEntry)((AMultiton)Activator.CreateInstance(type)).Fetch(key);
                }
            }
        }
        private static AMultiton FromCache(AMultitonKey key, Type type)
        {
            MethodInfo methodInfo = typeof(MultitonManager<>).MakeGenericType(type).GetMethod("FromInnerCache", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
            CreationFunction creationFunction = ((AMultiton)Activator.CreateInstance(type)).Fetch;
            return (AMultiton)methodInfo.Invoke(null, new object[2] { key, creationFunction });
        }
        private static AMultiton FromInnerCache(AMultitonKey key, CreationFunction creationFunction)
        {
            return HasCache ? MultitonCache<TEntry>.Instance(key, creationFunction) : MultitonTableStorage<TEntry>.Instance(key, creationFunction);
        }
        public static bool Update(AMultitonKey key, object value = null)
        {
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
            {
                MethodInfo methodInfo = (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Update", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
                if (value == null) value = ((AMultiton)Activator.CreateInstance(type)).Fetch(key);
                return (bool)methodInfo.Invoke(null, new object[2] { key, value });
            }
            else if (value != null)
            {
                string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
                if (!multitonDictionary.ContainsKey(innerKey)) multitonDictionary.Add(innerKey, null);
                multitonDictionary[innerKey] = (TEntry)value;
                return true;
            }
            else return false;
        }
        public static bool Exists(AMultitonKey key)
        {
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
            {
                MethodInfo methodInfo = (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Exists", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
                return (bool)methodInfo.Invoke(null, new object[2] { key, type });
            }
            else
            {
                string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
                return multitonDictionary.ContainsKey(innerKey);
            }
        }
        public static bool Delete(AMultitonKey key)
        {
            Type type = typeof(TEntry);
            if (HasCache || HasTableStorage)
            {
                MethodInfo methodInfo = (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("Delete", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
                return (bool)methodInfo.Invoke(null, new object[2] { key, type });
            }
            else
            {
                string innerKey = $"{type.FullName}{MultitonConst.Separator}{key.Value}";
                return multitonDictionary.Remove(innerKey);
            }
        }
        public static IEnumerable<AMultitonKey> List<TKey>() where TKey : AMultitonKey
        {
            Type type = typeof(TEntry);
            Type keyType = typeof(TKey);
            if (HasCache || HasTableStorage)
            {
                MethodInfo methodInfo = (HasCache ? typeof(MultitonCache<>) : typeof(MultitonTableStorage<>)).MakeGenericType(type).GetMethod("List", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
                IEnumerable<string> listedKeys = (IEnumerable<string>)methodInfo.Invoke(null, new object[1] { type });
                List<AMultitonKey> keys = new List<AMultitonKey>();
                foreach (string key in listedKeys)
                {
                    AMultitonKey multitonKey = (TKey)Activator.CreateInstance(keyType);
                    IEnumerator<string> keyValues = PropertyValue(key);
                    if (!keyValues.MoveNext()) continue;
                    foreach (PropertyInfo property in MultitonConst.PropertyInfoDictionary[keyType])
                    {
                        property.SetValue(multitonKey, Convert.ChangeType(keyValues.Current, property.PropertyType));
                        if (!keyValues.MoveNext()) break;
                    }
                    keys.Add(multitonKey);
                }
                return keys;
            }
            else
                throw new NotImplementedException();
            //todo: Alessandro Rapiti - da implementare anche la ricerca su lista per quando si ha solo multiton
            IEnumerator<string> PropertyValue(string key)
            {
                foreach (string s in key.Split(MultitonConst.Separator))
                    yield return s;
            }
        }
    }
    public class NoMultitonKey : Attribute { }
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
                            if (type.BaseType == MultitonConst.MultitonKey)
                            {
                                PropertyInfoDictionary.Add(type,
                                    type.GetProperties().ToList().FindAll(x =>
                                        x.GetCustomAttribute(MultitonConst.NoKey) == null && CheckPrimitiveList(x.PropertyType)));
                            }
                        }
                    }
                }
                catch (Exception er)
                {
                    string weee = er.ToString();
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
        public const char Separator = '_';
        public static Type NoKey = typeof(NoMultitonKey);
        public static Type MultitonKey = typeof(AMultitonKey);
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
