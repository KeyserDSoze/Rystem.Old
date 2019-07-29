using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rystem.Azure.Storage;

namespace System
{
    /// <summary>
    /// Dummy class
    /// </summary>
    internal class DummyTableStorage : TableEntity { }
    /// <summary>
    /// Context class
    /// </summary>
    public static class ExtensionTableStorage
    {
        #region Context
        private static object TrafficLight = new object();
        private static Dictionary<string, List<PropertyInfo>> Properties = new Dictionary<string, List<PropertyInfo>>();
        private static Dictionary<string, List<PropertyInfo>> SpecialProperties = new Dictionary<string, List<PropertyInfo>>();
        private static Dictionary<string, Dictionary<Installation, Dictionary<string, CloudTable>>> Contexts = new Dictionary<string, Dictionary<Installation, Dictionary<string, CloudTable>>>();
        private static CloudTable CreateContext(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable Context = tableClient.GetTableReference(tableName);
            Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return Context;
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
        private static void ContextExists(Type type, Installation installation = Installation.Default, string tableName = "")
        {
            if (!Contexts.ContainsKey(type.FullName))
                lock (TrafficLight)
                    if (!Contexts.ContainsKey(type.FullName))
                        Contexts.Add(type.FullName, new Dictionary<Installation, Dictionary<string, CloudTable>>());
            if (!Contexts[type.FullName].ContainsKey(installation))
            {
                lock (TrafficLight)
                {
                    if (!Contexts[type.FullName].ContainsKey(installation))
                    {
                        Dictionary<string, CloudTable> cloudContext = new Dictionary<string, CloudTable>();
                        var (connectionString, tableNames) = TableStorageInstaller.GetConnectionStringAndTableNames(type, installation);
                        foreach (string name in tableNames)
                            cloudContext.Add(name, CreateContext(connectionString, name));
                        PropertyExists(type);
                        Contexts[type.FullName].Add(installation, cloudContext);
                    }
                }
            }
        }
        private static CloudTable GetContext(Type type, Installation installation = Installation.Default, string tableName = "")
        {
            CloudTable context = null;
            ContextExists(type, installation, tableName);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                context = Contexts[type.FullName][installation][tableName];
            }
            else
            {
                context = Contexts[type.FullName][installation].FirstOrDefault().Value;
            }
            return context;
        }
        private static Dictionary<string, CloudTable> GetContextList(Type type, Installation installation = Installation.Default, string tableName = "")
        {
            CloudTable context = null;
            ContextExists(type, installation, tableName);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                context = Contexts[type.FullName][installation][tableName];
            }
            else
            {
                return Contexts[type.FullName][installation];
            }
            return new Dictionary<string, CloudTable>() { { type.Name, context } };
        }
        #endregion

        #region Async Methods
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
            where TEntity : ITableStorage
        {
            TableOperation operation = TableOperation.Retrieve<DummyTableStorage>(entity.PartitionKey, entity.RowKey);
            TableResult result = await GetContext(entity.GetType(), installation, tableName).ExecuteAsync(operation);
            return result.Result != null;
        }
        public static async Task<List<TEntity>> FetchAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null)
            where TEntity : ITableStorage
        {
            Type type = entity.GetType();
            List<TEntity> items = new List<TEntity>();
            TableContinuationToken token = null;
            CloudTable context = GetContext(type, installation, tableName);
            string query = ToQuery(expression?.Body);
            do
            {
                TableQuerySegment<DynamicTableEntity> seg = await context.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>() { FilterString = query, TakeCount = takeCount }, token);
                token = seg.ContinuationToken;
                items.AddRange(seg.Select(x => x.ReadEntity<TEntity>(type)));
                if (takeCount != null && items.Count >= takeCount) break;
                onProgress?.Invoke(items);
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }

        public static async Task<bool> UpdateAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
            where TEntity : ITableStorage
        {
            Type type = entity.GetType();
            Dictionary<string, CloudTable> pairs = GetContextList(type, installation, tableName);
            TableOperation operation = TableOperation.InsertOrReplace(entity.WriteEntity());
            bool returnCode = pairs.Count > 0;
            foreach (KeyValuePair<string, CloudTable> context in pairs)
                returnCode &= (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
             where TEntity : ITableStorage
        {
            Type type = entity.GetType();
            TableOperation operation = TableOperation.Delete(new DummyTableStorage()
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = "*"
            });
            Dictionary<string, CloudTable> pairs = GetContextList(type, installation, tableName);
            bool returnCode = pairs.Count > 0;
            foreach (KeyValuePair<string, CloudTable> context in pairs)
                returnCode &= (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
        #endregion

        #region Utility for Async Methods
        private static MethodInfo JsonConvertDeserializeMethod = typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList().FindAll(x => x.IsGenericMethod && x.Name.Equals("DeserializeObject") && x.GetParameters().ToList().FindAll(y => y.Name == "settings").Count > 0).FirstOrDefault();
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        private static TEntity ReadEntity<TEntity>(this DynamicTableEntity dynamicTableEntity, Type entityType)
            where TEntity : ITableStorage
        {
            TEntity entity = (TEntity)Activator.CreateInstance(entityType);
            entity.PartitionKey = dynamicTableEntity.PartitionKey;
            entity.RowKey = dynamicTableEntity.RowKey;
            entity.Timestamp = dynamicTableEntity.Timestamp.DateTime.ToUniversalTime();
            entity.ETag = dynamicTableEntity.ETag;
            foreach (PropertyInfo pi in Properties[entityType.FullName])
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                    SetValue(dynamicTableEntity.Properties[pi.Name], pi);
            foreach (PropertyInfo pi in SpecialProperties[entityType.FullName])
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                {
                    dynamic value = JsonConvertDeserializeMethod.MakeGenericMethod(pi.PropertyType).Invoke(null, new object[2] { dynamicTableEntity.Properties[pi.Name].StringValue, JsonSettings });
                    pi.SetValue(entity, value);
                }
            return entity;
            void SetValue(EntityProperty entityProperty, PropertyInfo pi)
            {
                switch (entityProperty.PropertyType)
                {
                    case EdmType.Int32:
                        pi.SetValue(entity, entityProperty.Int32Value.Value);
                        break;
                    case EdmType.Int64:
                        pi.SetValue(entity, entityProperty.Int64Value.Value);
                        break;
                    case EdmType.Guid:
                        pi.SetValue(entity, entityProperty.GuidValue.Value);
                        break;
                    case EdmType.Double:
                        pi.SetValue(entity, entityProperty.DoubleValue.Value);
                        break;
                    case EdmType.DateTime:
                        pi.SetValue(entity, entityProperty.DateTime.Value);
                        break;
                    case EdmType.Boolean:
                        pi.SetValue(entity, entityProperty.BooleanValue.Value);
                        break;
                    case EdmType.Binary:
                        pi.SetValue(entity, entityProperty.BinaryValue);
                        break;
                    case EdmType.String:
                        pi.SetValue(entity, entityProperty.StringValue);
                        break;
                }
            }
        }
        private static string ToQuery(Expression expression = null)
        {
            if (expression == null) return string.Empty;
            string result = QueryStrategy.Create(expression);
            if (!string.IsNullOrWhiteSpace(result)) return result;
            BinaryExpression binaryExpression = (BinaryExpression)expression;
            return ToQuery(binaryExpression.Left) + ExpressionTypeExtensions.MakeLogic(binaryExpression.NodeType) + ToQuery(binaryExpression.Right);
        }
        private static DateTime DateTimeDefault = default;
        private static DynamicTableEntity WriteEntity<TEntity>(this TEntity entity)
            where TEntity : ITableStorage
        {
            Type type = entity.GetType();
            DynamicTableEntity dummy = new DynamicTableEntity();
            dummy.PartitionKey = entity.PartitionKey;
            dummy.RowKey = entity.RowKey = entity.RowKey ?? string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
            dummy.Timestamp = entity.Timestamp > DateTimeDefault ? entity.Timestamp : (entity.Timestamp = DateTime.UtcNow);
            dummy.ETag = entity.ETag = entity.ETag ?? "*";
            foreach (PropertyInfo pi in Properties[type.FullName])
            {
                dynamic value = pi.GetValue(entity);
                if (value != null)
                    dummy.Properties.Add(pi.Name, new EntityProperty(value));
            }
            foreach (PropertyInfo pi in SpecialProperties[type.FullName])
                dummy.Properties.Add(pi.Name, new EntityProperty(
                    JsonConvert.SerializeObject(pi.GetValue(entity), JsonSettings)));
            return dummy;
        }
        #endregion

        #region Sync Methods
        public static bool Exists<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
             where TEntity : ITableStorage
        {
            return entity.ExistsAsync(installation, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static List<TEntity> Fetch<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, Installation installation = Installation.Default, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null)
             where TEntity : ITableStorage
        {
            return entity.FetchAsync(expression, takeCount, installation, tableName, ct, onProgress).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static bool Delete<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
             where TEntity : ITableStorage
        {
            return entity.DeleteAsync(installation, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static bool Update<TEntity>(this TEntity entity, Installation installation = Installation.Default, string tableName = "")
             where TEntity : ITableStorage
        {
            return entity.UpdateAsync(installation, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static List<string> ListOfTables<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : ITableStorage
        {
            Dictionary<string, CloudTable> pairs = GetContextList(entity.GetType(), installation);
            return pairs.Select(x => x.Value.Name).ToList();
        }
        #endregion
    }
}
