using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Rystem.Cache
{
    internal class MultitonTableStorage<TEntry> where TEntry : AMultiton
    {
        private static CloudTable Context;
        private static string ConnectionString;
        private static readonly object TrafficLight = new object();
        private static readonly object OnStartTrafficLight = new object();
        private static int ExpireCache = 0;
        private const string TableName = "ARCache";
        internal static void OnStart(string connectionString, int expireCache = 0)
        {
            ConnectionString = connectionString;
            ExpireCache = expireCache;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Context = tableClient.GetTableReference(TableName);
            Context.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }
        internal static TEntry Instance(AMultitonKey key, CreationFunction functionIfNotExists)
        {
            string partitionKey = typeof(TEntry).FullName;
            string rowKey = key.Value;
            ARCache cached = null;
            if ((cached = Exist(partitionKey, rowKey)) == null)
            {
                lock (TrafficLight)
                {
                    if ((cached = Exist(partitionKey, rowKey)) == null)
                    {
                        TEntry query = (TEntry)functionIfNotExists(key);
                        if (query != null)
                        {
                            Set(partitionKey, rowKey, JsonConvert.SerializeObject(query, MultitonConst.JsonSettings));
                            return query;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return JsonConvert.DeserializeObject<TEntry>(cached.Data, MultitonConst.JsonSettings);
        }
        private static ARCache Get(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<ARCache>(partitionKey, rowKey);
            TableResult result = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return (ARCache)result.Result;
        }
        private static ARCache Exist(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<ARCache>(partitionKey, rowKey);
            TableResult result = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            if (result.Result == null) return null;
            ARCache cached = (ARCache)result.Result;
            if (DateTime.UtcNow.Ticks - cached.Timestamp.UtcDateTime.Ticks > TimeSpan.FromMinutes(ExpireCache).Ticks) return null;
            return cached;
        }
        private static bool Set(string partitionKey, string rowKey, string data)
        {
            ARCache aRCache = new ARCache()
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Data = data
            };
            TableOperation operation = TableOperation.InsertOrReplace(aRCache);
            TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return (esito.HttpStatusCode == 204);
        }
        internal static bool Update(AMultitonKey key, TEntry value)
        {
            string partitionKey = typeof(TEntry).FullName;
            string rowKey = key.Value;
            return Set(partitionKey, rowKey, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings));
        }
        private static bool Remove(string partitionKey, string rowKey)
        {
            ARCache aRCache = new ARCache()
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                ETag = "*"
            };
            TableOperation operation = TableOperation.Delete(aRCache);
            TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return (esito.HttpStatusCode == 204);
        }
        internal static bool Delete(AMultitonKey key, Type type)
        {
            string partitionKey = type.FullName;
            string rowKey = key.Value;
            return Remove(partitionKey, rowKey);
        }
        internal static bool Exists(AMultitonKey key, Type type)
        {
            string partitionKey = type.FullName;
            string rowKey = key.Value;
            return Exist(partitionKey, rowKey) != null;
        }
        private static IEnumerable<string> Listing(string partitionKey)
        {
            TableQuery tableQuery = new TableQuery
            {
                FilterString = $"PartitionKey eq '{partitionKey}'"
            };
            TableContinuationToken tableContinuationToken = new TableContinuationToken();
            List<string> keys = new List<string>();
            do
            {
                TableQuerySegment tableQuerySegment = Context.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken).GetAwaiter().GetResult();
                IEnumerable<string> keysFromQuery = tableQuerySegment.Results.Select(x => x.RowKey);
                tableContinuationToken = tableQuerySegment.ContinuationToken;
                keys.AddRange(keysFromQuery);
            } while (tableContinuationToken != null);
            return keys;
        }
        internal static IEnumerable<string> List(Type type)
        {
            string partitionKey = type.FullName;
            return Listing(partitionKey);
        }
    }
    internal class ARCache : TableEntity
    {
        public string Data { get; set; }
    }
}
