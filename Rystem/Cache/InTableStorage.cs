using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Rystem.Cache;

namespace Rystem.Cache
{
    internal class InTableStorage<T> : AMultitonIntegration<T>
        where T : IMultiton
    {
        private static CloudTable Context;
        private static int ExpireCache = 0;
        private const string TableName = "RystemCache";
        private readonly static string FullName = typeof(T).FullName;
        internal InTableStorage(MultitonInstaller.MultitonConfiguration configuration)
        {
            ExpireCache = configuration.ExpireCache;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Context = tableClient.GetTableReference(TableName);
            Context.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }
        internal override T Instance(string key)
        {
            RystemCache cached = Exist(FullName, key);
            return JsonConvert.DeserializeObject<T>(cached.Data, MultitonConst.JsonSettings);
        }
        private static bool Set(string rowKey, string data)
        {
            RystemCache rystemCache = new RystemCache()
            {
                PartitionKey = FullName,
                RowKey = rowKey,
                Data = data
            };
            TableOperation operation = TableOperation.InsertOrReplace(rystemCache);
            TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return (esito.HttpStatusCode == 204);
        }
        internal override bool Update(string key, T value) => Set(key, JsonConvert.SerializeObject(value, MultitonConst.JsonSettings));
        internal override bool Delete(string key)
        {
            return Remove();
            bool Remove()
            {
                RystemCache rystemCache = new RystemCache()
                {
                    PartitionKey = FullName,
                    RowKey = key,
                    ETag = "*"
                };
                TableOperation operation = TableOperation.Delete(rystemCache);
                TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
                return (esito.HttpStatusCode == 204);
            }
        }
        internal override bool Exists(string key) => Exist(FullName, key) != null;
        private static RystemCache Exist(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(partitionKey, rowKey);
            TableResult result = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            if (result.Result == null) return null;
            RystemCache cached = (RystemCache)result.Result;
            if (ExpireCache > 0 && DateTime.UtcNow.Ticks - cached.Timestamp.UtcDateTime.Ticks > TimeSpan.FromMinutes(ExpireCache).Ticks) return null;
            return cached;
        }
        internal override IEnumerable<string> List()
        {
            return Listing();
            IEnumerable<string> Listing()
            {
                TableQuery tableQuery = new TableQuery
                {
                    FilterString = $"PartitionKey eq '{FullName}'"
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
        }
        private class RystemCache : TableEntity
        {
            public string Data { get; set; }
        }
    }
}
