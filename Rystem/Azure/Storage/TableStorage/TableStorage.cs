using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Azure.Storage
{
    /// <summary>
    /// Implementation of ITableEntity
    /// </summary>
    public abstract partial class ATableStorage : ITableEntity
    {
        public string PartitionKey { get; set; }
        private string rowKey;
        public string RowKey
        {
            get
            {
                return rowKey ?? (rowKey = TableStorageUtility.RandomTimedRowKey);
            }
            set
            {
                rowKey = value;
            }
        }
        private DateTimeOffset? timestamp;
        public DateTimeOffset Timestamp
        {
            get
            {
                return timestamp ?? (timestamp = DateTimeOffset.UtcNow) ?? DateTimeOffset.UtcNow;
            }
            set
            {
                timestamp = value;
            }
        }
        public string ETag { get; set; } = "*";
        private static MethodInfo JsonConvertDeserializeMethod = typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList().FindAll(x => x.IsGenericMethod && x.Name.Equals("DeserializeObject") && x.GetParameters().ToList().FindAll(y => y.Name == "settings").Count > 0).FirstOrDefault();
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            Type type = this.GetType();
            foreach (PropertyInfo pi in Properties[type.FullName])
                if (properties.ContainsKey(pi.Name))
                    GetEntity(properties[pi.Name], pi);
            foreach (PropertyInfo pi in SpecialProperties[type.FullName])
                if (properties.ContainsKey(pi.Name))
                {
                    dynamic value = JsonConvertDeserializeMethod.MakeGenericMethod(pi.PropertyType).Invoke(null, new object[2] { properties[pi.Name].StringValue, JsonSettings });
                    pi.SetValue(this, value);
                }
            void GetEntity(EntityProperty entityProperty, PropertyInfo pi)
            {
                switch (entityProperty.PropertyType)
                {
                    case EdmType.Int32:
                        pi.SetValue(this, entityProperty.Int32Value.Value);
                        break;
                    case EdmType.Int64:
                        pi.SetValue(this, entityProperty.Int64Value.Value);
                        break;
                    case EdmType.Guid:
                        pi.SetValue(this, entityProperty.GuidValue.Value);
                        break;
                    case EdmType.Double:
                        pi.SetValue(this, entityProperty.DoubleValue.Value);
                        break;
                    case EdmType.DateTime:
                        pi.SetValue(this, entityProperty.DateTime.Value);
                        break;
                    case EdmType.Boolean:
                        pi.SetValue(this, entityProperty.BooleanValue.Value);
                        break;
                    case EdmType.Binary:
                        pi.SetValue(this, entityProperty.BinaryValue);
                        break;
                    case EdmType.String:
                        pi.SetValue(this, entityProperty.StringValue);
                        break;
                }
            }
        }
        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            Type type = this.GetType();
            IDictionary<string, EntityProperty> properties = new Dictionary<string, EntityProperty>();
            foreach (PropertyInfo pi in Properties[type.FullName])
            {
                dynamic value = pi.GetValue(this);
                properties.Add(pi.Name, new EntityProperty(value));
            }
            foreach (PropertyInfo pi in SpecialProperties[type.FullName])
                properties.Add(pi.Name, new EntityProperty(
                    JsonConvert.SerializeObject(pi.GetValue(this), JsonSettings)));
            return properties;
        }
    }
    /// <summary>
    /// Constructor and installer
    /// </summary>
    public abstract partial class ATableStorage
    {
        private static string ConnectionStringDefault;
        private static object TrafficLight = new object();
        private static Dictionary<string, List<PropertyInfo>> Properties = new Dictionary<string, List<PropertyInfo>>();
        private static Dictionary<string, List<PropertyInfo>> SpecialProperties = new Dictionary<string, List<PropertyInfo>>();
        private static Dictionary<string, Dictionary<string, CloudTable>> Contexts = new Dictionary<string, Dictionary<string, CloudTable>>();
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
        private static CloudTable GetContext(Type type, string tableName = "")
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
        private static Dictionary<string, CloudTable> GetContextList(Type type, string tableName = "")
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
    /// <summary>
    /// Async Methods
    /// </summary>
    public abstract partial class ATableStorage
    {
        public static async Task<bool> ExistsAsync<TEntity>(TEntity entity, string tableName = "") where TEntity : ATableStorage, new()
        {
            if (string.IsNullOrWhiteSpace(entity.PartitionKey) && string.IsNullOrWhiteSpace(entity.RowKey)) return false;
            Type type = typeof(TEntity);
            return (await FetchAsync<TEntity>(x => x.PartitionKey == entity.PartitionKey && x.RowKey == entity.RowKey, 1)).FirstOrDefault() != null;
        }
        public static async Task<List<TEntity>> FetchAsync<TEntity>(Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null) where TEntity : ATableStorage, new()
        {
            Type type = typeof(TEntity);
            List<TEntity> items = new List<TEntity>();
            TableContinuationToken token = null;
            CloudTable context = GetContext(type, tableName);
            string query = ToQuery(expression?.Body);
            do
            {
                TableQuerySegment<TEntity> seg = await context.ExecuteQuerySegmentedAsync<TEntity>(new TableQuery<TEntity>() { FilterString = query, TakeCount = takeCount }, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                if (takeCount != null && items.Count >= takeCount) break;
                onProgress?.Invoke(items);
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }
        private static string ToQuery(Expression expression = null)
        {
            if (expression == null) return string.Empty;
            string result = QueryStrategy.Create(expression);
            if (!string.IsNullOrWhiteSpace(result)) return result;
            BinaryExpression binaryExpression = (BinaryExpression)expression;
            return ToQuery(binaryExpression.Left) + ExpressionTypeExtensions.MakeLogic(binaryExpression.NodeType) + ToQuery(binaryExpression.Right);
        }
        public async Task<bool> UpdateAsync(string tableName = "")
        {
            Type type = this.GetType();
            TableOperation operation = TableOperation.InsertOrReplace(this);
            bool returnCode = false;
            foreach (KeyValuePair<string, CloudTable> context in GetContextList(type, tableName))
                returnCode = (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
        public async Task<bool> DeleteAsync(string tableName = "")
        {
            Type type = this.GetType();
            TableOperation operation = TableOperation.Delete(this);
            bool returnCode = false;
            foreach (KeyValuePair<string, CloudTable> context in GetContextList(type, tableName))
                returnCode = (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
    }
    /// <summary>
    /// Sync Methods
    /// </summary>
    public abstract partial class ATableStorage
    {
        public static bool Exists<TEntity>(TEntity entity, string tableName = "") where TEntity : ATableStorage, new()
        {
            return ExistsAsync(entity, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static List<TEntity> Fetch<TEntity>(Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null) where TEntity : ATableStorage, new()
        {
            return FetchAsync(expression, takeCount, tableName, ct, onProgress).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public bool Delete(string tableName = "")
        {
            return DeleteAsync(tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public bool Update(string tableName = "")
        {
            return UpdateAsync(tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
    /// <summary>
    /// Utility per gestire al meglio <see cref="TableStorage{TEntity}"/>
    /// </summary>
    public class TableStorageUtility
    {
        /// <summary>
        /// Ottieni una chiave per la tua RowKey randomica, sempre diversa, di modo da avere per PartitionKey
        /// le istanze ordinate in maniera temporalmente decrescente, l'ultima entrata è la prima che vedo.
        /// </summary>
        public static string RandomTimedRowKey
        {
            get
            {
                return string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
            }
        }
    }
    /// <summary>
    /// Metodi di estensione.
    /// </summary>
    
}
