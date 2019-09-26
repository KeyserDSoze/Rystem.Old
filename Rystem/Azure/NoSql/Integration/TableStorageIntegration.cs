using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal class TableStorageIntegration<TEntity> : INoSqlIntegration<TEntity>
        where TEntity : INoSql
    {
        private CloudTable Context;
        private IList<PropertyInfo> Properties = new List<PropertyInfo>();
        private IList<PropertyInfo> SpecialProperties = new List<PropertyInfo>();
        internal TableStorageIntegration(NoSqlConfiguration noSqlConfiguration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(noSqlConfiguration.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            this.Context = tableClient.GetTableReference(noSqlConfiguration.Name);
            this.Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (PropertyInfo pi in typeof(TEntity).GetProperties())
            {
                if (pi.GetCustomAttribute(typeof(NoTableStorageProperty)) != null)
                    continue;
                if (pi.Name == "PartitionKey" || pi.Name == "RowKey" || pi.Name == "Timestamp" || pi.Name == "ETag")
                    continue;
                if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(long) ||
                    pi.PropertyType == typeof(double) || pi.PropertyType == typeof(string) ||
                    pi.PropertyType == typeof(Guid) || pi.PropertyType == typeof(bool) ||
                    pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(byte[]))
                    Properties.Add(pi);
                else
                    SpecialProperties.Add(pi);
            }
        }
        private class DummyTableStorage : TableEntity { }
        public async Task<bool> DeleteAsync(INoSql entity)
        {
            TableOperation operation = TableOperation.Delete(new DummyTableStorage()
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = "*"
            });
            return (await this.Context.ExecuteAsync(operation)).HttpStatusCode == 204;
        }

        public async Task<bool> ExistsAsync(INoSql entity)
        {
            TableOperation operation = TableOperation.Retrieve<DummyTableStorage>(entity.PartitionKey, entity.RowKey);
            TableResult result = await this.Context.ExecuteAsync(operation);
            return result.Result != null;
        }

        public async Task<IList<TEntity>> GetAsync(INoSql entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null)
        {
            List<TEntity> items = new List<TEntity>();
            TableContinuationToken token = null;
            string query = ToQuery(expression?.Body);
            do
            {
                TableQuerySegment<DynamicTableEntity> seg = await this.Context.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>() { FilterString = query, TakeCount = takeCount }, token);
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

        public async Task<bool> UpdateAsync(INoSql entity)
        {
            TableOperation operation = TableOperation.InsertOrReplace(WriteEntity(entity));
            return (await this.Context.ExecuteAsync(operation)).HttpStatusCode == 204;
        }
        private static DateTime DateTimeDefault = default;
        private DynamicTableEntity WriteEntity(INoSql entity)
        {
            DynamicTableEntity dummy = new DynamicTableEntity();
            dummy.PartitionKey = entity.PartitionKey;
            dummy.RowKey = entity.RowKey = entity.RowKey ?? string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
            dummy.Timestamp = entity.Timestamp > DateTimeDefault ? entity.Timestamp : (entity.Timestamp = DateTime.UtcNow);
            dummy.ETag = entity.Tag = entity.Tag ?? "*";
            foreach (PropertyInfo pi in this.Properties)
            {
                dynamic value = pi.GetValue(entity);
                if (value != null)
                    dummy.Properties.Add(pi.Name, new EntityProperty(value));
            }
            foreach (PropertyInfo pi in this.SpecialProperties)
                dummy.Properties.Add(pi.Name, new EntityProperty(
                    JsonConvert.SerializeObject(pi.GetValue(entity), JsonSettings)));
            return dummy;
        }
        private static MethodInfo JsonConvertDeserializeMethod = typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).First(x => x.IsGenericMethod && x.Name.Equals("DeserializeObject") && x.GetParameters().ToList().FindAll(y => y.Name == "settings").Count > 0);
        private TEntity ReadEntity(DynamicTableEntity dynamicTableEntity)
        {
            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            entity.PartitionKey = dynamicTableEntity.PartitionKey;
            entity.RowKey = dynamicTableEntity.RowKey;
            entity.Timestamp = dynamicTableEntity.Timestamp.DateTime.ToUniversalTime();
            foreach (PropertyInfo pi in this.Properties)
                if (dynamicTableEntity.Properties.ContainsKey(pi.Name))
                    SetValue(dynamicTableEntity.Properties[pi.Name], pi);
            foreach (PropertyInfo pi in this.SpecialProperties)
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

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
