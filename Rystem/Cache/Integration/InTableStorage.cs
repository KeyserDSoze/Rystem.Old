using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Rystem.Cache;
using Rystem.Const;

namespace Rystem.Cache
{
    internal class InTableStorage<T> : IMultitonIntegrationAsync<T>
    {
        private static CloudTable Context;
        private static long ExpireCache = 0;
        private const string TableName = "RystemCache";
        private readonly static string FullName = typeof(T).FullName;
        private readonly CacheProperties Properties;
        internal InTableStorage(RystemCacheProperty configuration)
        {
            Properties = configuration.CloudProperties;
            ExpireCache = Properties.ExpireTimeSpan.Ticks;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Context = tableClient.GetTableReference(TableName);
            Context.CreateIfNotExistsAsync().ToResult();
        }
        public async Task<T> InstanceAsync(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = await Context.ExecuteAsync(operation).NoContext();
            return result.Result != default ? ((RystemCache)result.Result).Data.FromDefaultJson<T>() : default;
        }
        public async Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime)
        {
            long expiring = ExpireCache;
            if (expiringTime != default)
                expiring = expiringTime.Ticks;
            RystemCache rystemCache = new RystemCache()
            {
                PartitionKey = FullName,
                RowKey = key,
                Data = value.ToDefaultJson(),
                E = expiring > 0 ? expiring + DateTime.UtcNow.Ticks : DateTime.MaxValue.Ticks
            };
            TableOperation operation = TableOperation.InsertOrReplace(rystemCache);
            TableResult result = await Context.ExecuteAsync(operation).NoContext();
            return result.HttpStatusCode == 204;
        }
        public async Task<bool> DeleteAsync(string key)
        {
            RystemCache rystemCache = new RystemCache()
            {
                PartitionKey = FullName,
                RowKey = key,
                ETag = "*"
            };
            TableOperation operation = TableOperation.Delete(rystemCache);
            try
            {
                TableResult result = await Context.ExecuteAsync(operation).NoContext();
                return result.HttpStatusCode == 204;
            }
            catch (StorageException er)
            {
                if (er.HResult == -2146233088)
                    return true;
                throw er;
            }
        }
        public async Task<MultitonStatus<T>> ExistsAsync(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = await Context.ExecuteAsync(operation).NoContext();
            if (result.Result == null)
                return MultitonStatus<T>.NotOk();
            RystemCache cached = (RystemCache)result.Result;
            if (DateTime.UtcNow.Ticks > cached.E)
            {
                await this.DeleteAsync(key).NoContext();
                return MultitonStatus<T>.NotOk();
            }
            return MultitonStatus<T>.Ok(cached.Data.FromDefaultJson<T>());
        }
        public async Task<IEnumerable<string>> ListAsync()
        {
            TableQuery tableQuery = new TableQuery
            {
                FilterString = $"PartitionKey eq '{FullName}'"
            };
            TableContinuationToken tableContinuationToken = new TableContinuationToken();
            List<string> keys = new List<string>();
            do
            {
                TableQuerySegment tableQuerySegment = await Context.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken).NoContext();
                IEnumerable<string> keysFromQuery = tableQuerySegment.Results.Select(x => x.RowKey);
                tableContinuationToken = tableQuerySegment.ContinuationToken;
                keys.AddRange(keysFromQuery);
            } while (tableContinuationToken != null);
            return keys;
        }
        public Task WarmUp() 
            => Task.CompletedTask;
        private class RystemCache : TableEntity
        {
            public string Data { get; set; }
            public long E { get; set; }
        }
    }
}
