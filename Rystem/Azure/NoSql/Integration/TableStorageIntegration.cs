using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.NoSql
{
    internal class TableStorageIntegration<TEntity> : INoSqlIntegration<TEntity>
    {
        private CloudTable context;
        private readonly RaceCondition RaceCondition = new RaceCondition();
        private async Task<CloudTable> GetContextAsync()
        {
            if (context == null)
                await RaceCondition.ExecuteAsync(async () =>
                {
                    if (context == null)
                    {
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(NoSqlConfiguration.ConnectionString);
                        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                        var preContext = tableClient.GetTableReference(NoSqlConfiguration.Name ?? EntityType.Name);
                        if (!await preContext.ExistsAsync().NoContext())
                            await preContext.CreateIfNotExistsAsync().NoContext();
                        context = preContext;
                    }
                }).NoContext();
            return context;
        }
        private readonly IDictionary<string, PropertyInfo> BaseProperties = new Dictionary<string, PropertyInfo>();
        private readonly IList<PropertyInfo> Properties = new List<PropertyInfo>();
        private readonly IList<PropertyInfo> SpecialProperties = new List<PropertyInfo>();
        private const string PartitionKey = "PartitionKey";
        private const string RowKey = "RowKey";
        private const string Timestamp = "Timestamp";
        private const string ETag = "ETag";
        private readonly NoSqlConfiguration NoSqlConfiguration;
        private readonly Type EntityType;
        internal TableStorageIntegration(NoSqlConfiguration noSqlConfiguration, TEntity entity)
        {
            this.EntityType = entity.GetType();
            this.NoSqlConfiguration = noSqlConfiguration;
            foreach (PropertyInfo pi in this.EntityType.GetProperties())
            {
                if (pi.GetCustomAttribute(typeof(NoSqlProperty)) != null)
                    continue;
                if (pi.Name == PartitionKey || pi.Name == RowKey || pi.Name == Timestamp || pi.Name == ETag)
                    BaseProperties.Add(pi.Name, pi);
                else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(long) ||
                    pi.PropertyType == typeof(double) || pi.PropertyType == typeof(string) ||
                    pi.PropertyType == typeof(Guid) || pi.PropertyType == typeof(bool) ||
                    pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(byte[]))
                    Properties.Add(pi);
                else
                    SpecialProperties.Add(pi);
            }
        }
        private DynamicTableEntity GetBase(TEntity entity)
        {
            object partitionKey = this.BaseProperties[PartitionKey].GetValue(entity);
            object rowKey = this.BaseProperties[RowKey].GetValue(entity);
            object eTag = this.BaseProperties[ETag].GetValue(entity);
            return new DynamicTableEntity()
            {
                PartitionKey = (partitionKey ?? DateTime.UtcNow.ToString("yyyyMMdd")).ToString(),
                RowKey = (rowKey ?? Utility.Alea.GetTimedKey()).ToString(),
                ETag = eTag == null ? "*" : eTag.ToString(),
            };
        }
        public async Task<bool> DeleteAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync();
            TableOperation operation = TableOperation.Delete(this.GetBase(entity));
            return (await client.ExecuteAsync(operation).NoContext()).HttpStatusCode == 204;
        }

        public async Task<bool> ExistsAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync();
            DynamicTableEntity tableStorage = this.GetBase(entity);
            TableOperation operation = TableOperation.Retrieve<DynamicTableEntity>(tableStorage.PartitionKey, tableStorage.RowKey);
            TableResult result = await client.ExecuteAsync(operation).NoContext();
            return result.Result != null;
        }

        public async Task<IList<TEntity>> GetAsync(TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null)
        {
            var client = context ?? await GetContextAsync();
            List<TEntity> items = new List<TEntity>();
            TableContinuationToken token = null;
            string query = ToQuery(expression?.Body);
            do
            {
                TableQuerySegment<DynamicTableEntity> seg = await client.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>() { FilterString = query, TakeCount = takeCount }, token).NoContext();
                token = seg.ContinuationToken;
                items.AddRange(seg.Select(x => ReadEntity(x)));
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null);
            return items;

            string ToQuery(Expression expressionAsExpression = null)
            {
                if (expressionAsExpression == null)
                    return string.Empty;
                string result = QueryStrategy.Create(expressionAsExpression);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
                BinaryExpression binaryExpression = (BinaryExpression)expressionAsExpression;
                return ToQuery(binaryExpression.Left) + ExpressionTypeExtensions.MakeLogic(binaryExpression.NodeType) + ToQuery(binaryExpression.Right);
            }
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync();
            TableOperation operation = TableOperation.InsertOrReplace(WriteEntity(entity));
            return (await client.ExecuteAsync(operation).NoContext()).HttpStatusCode == 204;
        }
        public async Task<bool> UpdateBatchAsync(IEnumerable<TEntity> entities)
        {
            var client = context ?? await GetContextAsync();
            bool result = true;
            foreach (var groupedEntity in entities.Select(x => this.WriteEntity(x)).GroupBy(x => x.PartitionKey))
            {
                TableBatchOperation batch = new TableBatchOperation();
                foreach (DynamicTableEntity entity in groupedEntity)
                {
                    batch.InsertOrReplace(entity);
                    if (batch.Count == 100)
                    {
                        IList<TableResult> results = await client.ExecuteBatchAsync(batch).NoContext();
                        result &= results.All(x => x.HttpStatusCode == 204);
                        batch = new TableBatchOperation();
                    }
                }
                if (batch.Count > 0)
                    result &= (await client.ExecuteBatchAsync(batch).NoContext()).All(x => x.HttpStatusCode == 204);
            }
            return result;
        }
        public async Task<bool> DeleteBatchAsync(IEnumerable<TEntity> entities)
        {
            var client = context ?? await GetContextAsync();
            bool result = true;
            foreach (var groupedEntity in entities.Select(x => this.GetBase(x)).GroupBy(x => x.PartitionKey))
            {
                TableBatchOperation batch = new TableBatchOperation();
                foreach (DynamicTableEntity entity in groupedEntity)
                {
                    batch.Delete(entity);
                    if (batch.Count == 100)
                    {
                        IList<TableResult> results = await client.ExecuteBatchAsync(batch).NoContext();
                        result &= (results.All(x => x.HttpStatusCode == 204));
                        batch = new TableBatchOperation();
                    }
                }
                if (batch.Count > 0)
                    result &= (await client.ExecuteBatchAsync(batch).NoContext()).All(x => x.HttpStatusCode == 204);
            }
            return result;
        }
        private DynamicTableEntity WriteEntity(TEntity entity)
        {
            DynamicTableEntity dynamicTableEntity = this.GetBase(entity);
            foreach (PropertyInfo pi in this.Properties)
            {
                dynamic value = pi.GetValue(entity);
                if (value != null)
                    dynamicTableEntity.Properties.Add(pi.Name, new EntityProperty(value));
            }
            foreach (PropertyInfo pi in this.SpecialProperties)
                dynamicTableEntity.Properties.Add(pi.Name, new EntityProperty(
                    pi.GetValue(entity).ToDefaultJson()));
            return dynamicTableEntity;
        }
        private static readonly MethodInfo JsonConvertDeserializeMethod = typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).First(x => x.IsGenericMethod && x.Name.Equals("DeserializeObject") && x.GetParameters().ToList().FindAll(y => y.Name == "settings").Count > 0);
        private TEntity ReadEntity(DynamicTableEntity dynamicTableEntity)
        {
            TEntity entity = (TEntity)Activator.CreateInstance(this.EntityType);
            this.BaseProperties[PartitionKey].SetValue(entity, dynamicTableEntity.PartitionKey);
            this.BaseProperties[RowKey].SetValue(entity, dynamicTableEntity.RowKey);
            this.BaseProperties[Timestamp].SetValue(entity, dynamicTableEntity.Timestamp.DateTime.ToUniversalTime());
            foreach (PropertyInfo pi in this.Properties)
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                    SetValue(dynamicTableEntity.Properties[pi.Name], pi);
            foreach (PropertyInfo pi in this.SpecialProperties)
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                {
                    dynamic value = JsonConvertDeserializeMethod.MakeGenericMethod(pi.PropertyType).Invoke(null, new object[2] { dynamicTableEntity.Properties[pi.Name].StringValue, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings });
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
    }
}