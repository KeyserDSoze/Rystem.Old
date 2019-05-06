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
    /// Dummy class
    /// </summary>
    internal class DummyTableStorage : TableEntity { }
    /// <summary>
    /// Async Methods
    /// </summary>
    public static partial class ExternalTableStorage
    {
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity, string tableName = "")
            where TEntity : ITableStorage, new()
        {
            TableOperation operation = TableOperation.Retrieve<DummyTableStorage>(entity.PartitionKey, entity.RowKey);
            TableResult result = await TableStorageInstall.GetContext(entity.GetType(), tableName).ExecuteAsync(operation);
            return result.Result != null;
        }
        public static async Task<List<TEntity>> FetchAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null)
            where TEntity : ITableStorage, new()
        {
            Type type = typeof(TEntity);
            List<TEntity> items = new List<TEntity>();
            TableContinuationToken token = null;
            CloudTable context = TableStorageInstall.GetContext(type, tableName);
            string query = ToQuery(expression?.Body);
            do
            {
                TableQuerySegment<DynamicTableEntity> seg = await context.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>() { FilterString = query, TakeCount = takeCount }, token);
                token = seg.ContinuationToken;
                items.AddRange(seg.Select(x => ReadEntity<TEntity>(x, type)));
                if (takeCount != null && items.Count >= takeCount) break;
                onProgress?.Invoke(items);
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }

        public static async Task<bool> UpdateAsync(this ITableStorage entity, string tableName = "")
        {
            Type type = entity.GetType();
            TableOperation operation = TableOperation.InsertOrReplace(WriteEntity(entity, type));
            bool returnCode = false;
            foreach (KeyValuePair<string, CloudTable> context in TableStorageInstall.GetContextList(type, tableName))
                returnCode = (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
        public static async Task<bool> DeleteAsync(this ITableStorage entity, string tableName = "")
        {
            Type type = entity.GetType();
            TableOperation operation = TableOperation.Delete(new DummyTableStorage()
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = "*"
            });
            bool returnCode = false;
            foreach (KeyValuePair<string, CloudTable> context in TableStorageInstall.GetContextList(type, tableName))
                returnCode = (await context.Value.ExecuteAsync(operation)).HttpStatusCode == 204;
            return returnCode;
        }
    }
    /// <summary>
    /// Utility for Async
    /// </summary>
    public static partial class ExternalTableStorage
    {
        private static MethodInfo JsonConvertDeserializeMethod = typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList().FindAll(x => x.IsGenericMethod && x.Name.Equals("DeserializeObject") && x.GetParameters().ToList().FindAll(y => y.Name == "settings").Count > 0).FirstOrDefault();
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        private static TEntity ReadEntity<TEntity>(DynamicTableEntity dynamicTableEntity, Type type)
            where TEntity : ITableStorage, new()
        {
            TEntity entity = Activator.CreateInstance<TEntity>();
            entity.PartitionKey = dynamicTableEntity.PartitionKey;
            entity.RowKey = dynamicTableEntity.RowKey;
            entity.Timestamp = dynamicTableEntity.Timestamp;
            entity.ETag = dynamicTableEntity.ETag;
            foreach (PropertyInfo pi in TableStorageInstall.Properties[type.FullName])
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                    SetValue(dynamicTableEntity.Properties[pi.Name], pi);
            foreach (PropertyInfo pi in TableStorageInstall.SpecialProperties[type.FullName])
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
        private static DateTimeOffset DateTimeOffsetDefault = default(DateTimeOffset);
        private static DynamicTableEntity WriteEntity(ITableStorage entity, Type type)
        {
            DynamicTableEntity dummy = new DynamicTableEntity();
            dummy.PartitionKey = entity.PartitionKey;
            dummy.RowKey = string.IsNullOrWhiteSpace(entity.RowKey) ? (entity.RowKey = string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"))) : entity.RowKey;
            dummy.Timestamp = entity.Timestamp == DateTimeOffsetDefault ? (entity.Timestamp = DateTimeOffset.UtcNow) : entity.Timestamp;
            dummy.ETag = string.IsNullOrWhiteSpace(entity.ETag) ? (entity.ETag = "*") : entity.ETag;
            foreach (PropertyInfo pi in TableStorageInstall.Properties[type.FullName])
            {
                dynamic value = pi.GetValue(entity);
                dummy.Properties.Add(pi.Name, new EntityProperty(value));
            }
            foreach (PropertyInfo pi in TableStorageInstall.SpecialProperties[type.FullName])
                dummy.Properties.Add(pi.Name, new EntityProperty(
                    JsonConvert.SerializeObject(pi.GetValue(entity), JsonSettings)));
            return dummy;
        }
    }
    /// <summary>
    /// Sync methods
    /// </summary>
    public static partial class ExternalTableStorage
    {
        public static bool Exists<TEntity>(this TEntity entity, string tableName = "")
            where TEntity : ITableStorage, new()
        {
            return entity.ExistsAsync(tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static List<TEntity> Fetch<TEntity>(this TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null, string tableName = "", CancellationToken ct = default(CancellationToken), Action<IList<TEntity>> onProgress = null)
            where TEntity : ITableStorage, new()
        {
            return entity.FetchAsync(expression, takeCount, tableName, ct, onProgress).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static bool Delete<TEntity>(this TEntity entity, string tableName = "")
             where TEntity : ITableStorage, new()
        {
            return entity.DeleteAsync(tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public static bool Update<TEntity>(this TEntity entity, string tableName = "")
             where TEntity : ITableStorage, new()
        {
            return entity.UpdateAsync(tableName).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
