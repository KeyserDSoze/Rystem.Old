using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rystem.Azure.Storage
{
    public static class TableStorageInstall
    {
        private static string ConnectionStringDefault;
        private static object TrafficLight = new object();
        internal static Dictionary<string, List<PropertyInfo>> Properties = new Dictionary<string, List<PropertyInfo>>();
        internal static Dictionary<string, List<PropertyInfo>> SpecialProperties = new Dictionary<string, List<PropertyInfo>>();
        internal static Dictionary<string, Dictionary<string, CloudTable>> Contexts = new Dictionary<string, Dictionary<string, CloudTable>>();
        public static void Install(string connectionString)
        {
            ConnectionStringDefault = connectionString;
        }
        public static void Install<Entity>(string connectionString, params string[] tableNames) where Entity : new()
        {
            InstallAsync<Entity>(connectionString, tableNames).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static async Task InstallAsync<Entity>(string connectionString, params string[] tableNames) where Entity : new()
        {
            Type type = typeof(Entity);
            if (!Contexts.ContainsKey(type.FullName))
            {
                Contexts.Add(type.FullName, new Dictionary<string, CloudTable>());
                List<string> names = tableNames?.ToList();
                if (names.Count == 0) names.Add(type.Name);
                foreach (string tableName in names)
                    Contexts[type.FullName].Add(tableName, await CreateContextAsync(connectionString, tableName));
            }
            PropertyExists(type);
        }
        private static CloudTable CreateContext(string connectionString, string tableName)
        {
            return CreateContextAsync(connectionString, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        private static async Task<CloudTable> CreateContextAsync(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable Context = tableClient.GetTableReference(tableName);
            await Context.CreateIfNotExistsAsync();
            return Context;
        }
        internal static CloudTable GetContext(Type type, string tableName = "")
        {
            CloudTable context = null;
            ContextExists(type, tableName);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                context = Contexts[type.FullName][tableName];
            }
            else
            {
                context = Contexts[type.FullName].FirstOrDefault().Value;
            }
            return context;
        }
        internal static Dictionary<string, CloudTable> GetContextList(Type type, string tableName = "")
        {
            CloudTable context = null;
            ContextExists(type, tableName);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                context = Contexts[type.FullName][tableName];
            }
            else
            {
                return Contexts[type.FullName];
            }
            return new Dictionary<string, CloudTable>() { { type.Name, context } };
        }
        private static void PropertyExists(Type type)
        {
            if (!Properties.ContainsKey(type.FullName))
            {
                List<PropertyInfo> propertyInfo = new List<PropertyInfo>();
                List<PropertyInfo> specialPropertyInfo = new List<PropertyInfo>();
                foreach (PropertyInfo pi in type.GetProperties())
                {
                    if (pi.Name == "PartitionKey" || pi.Name == "RowKey" || pi.Name == "Timestamp" || pi.Name == "ETag")
                        continue;
                    if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(long) ||
                        pi.PropertyType == typeof(double) || pi.PropertyType == typeof(string) ||
                        pi.PropertyType == typeof(Guid) || pi.PropertyType == typeof(bool) ||
                        pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(byte[]))
                    {
                        propertyInfo.Add(pi);
                    }
                    else
                    {
                        specialPropertyInfo.Add(pi);
                    }
                }
                Properties.Add(type.FullName, propertyInfo);
                SpecialProperties.Add(type.FullName, specialPropertyInfo);
            }
        }
        private static void ContextExists(Type type, string tableName = "")
        {
            if (!Contexts.ContainsKey(type.FullName))
            {
                Activator.CreateInstance(type);
                if (!Contexts.ContainsKey(type.FullName))
                {
                    if (!string.IsNullOrWhiteSpace(ConnectionStringDefault))
                    {
                        lock (TrafficLight)
                        {
                            if (!Contexts.ContainsKey(type.FullName))
                            {
                                Contexts.Add(type.FullName, new Dictionary<string, CloudTable>());
                                Contexts[type.FullName].Add(type.Name, CreateContext(ConnectionStringDefault, type.Name));
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Please use Install static method in static constructor of your class to set ConnectionString and names of table");
                    }
                }
                PropertyExists(type);
            }
        }
    }
}
