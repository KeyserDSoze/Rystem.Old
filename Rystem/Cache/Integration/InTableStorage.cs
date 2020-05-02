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
    internal class InTableStorage<T> : IMultitonIntegration<T>
        where T : IMultiton, new()
    {
        private static CloudTable Context;
        private static long ExpireCache = 0;
        private const string TableName = "RystemCache";
        private readonly static string FullName = typeof(T).FullName;
        internal InTableStorage(InCloudMultitonProperties configuration)
        {
            ExpireCache = configuration.ExpireTimeSpan.Ticks;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Context = tableClient.GetTableReference(TableName);
            Context.CreateIfNotExistsAsync().NoContext().GetAwaiter().GetResult();
        }
        public T Instance(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = Context.ExecuteAsync(operation).NoContext().GetAwaiter().GetResult();
            return result.Result != default ? ((RystemCache)result.Result).Data.FromStandardJson<T>() : default;
        }
        public bool Update(string key, T value, TimeSpan expiringTime)
        {
            long expiring = ExpireCache;
            if (expiringTime != default)
                expiring = expiringTime.Ticks;
            RystemCache rystemCache = new RystemCache()
            {
                PartitionKey = FullName,
                RowKey = key,
                Data = value.ToStandardJson(),
                E = expiring > 0 ? expiring + DateTime.UtcNow.Ticks : DateTime.MaxValue.Ticks
            };
            TableOperation operation = TableOperation.InsertOrReplace(rystemCache);
            TableResult esito = Context.ExecuteAsync(operation).NoContext().GetAwaiter().GetResult();
            return (esito.HttpStatusCode == 204);
        }
        public bool Delete(string key)
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
                TableResult esito = Context.ExecuteAsync(operation).NoContext().GetAwaiter().GetResult();
                return (esito.HttpStatusCode == 204);
            }
            catch (StorageException er)
            {
                if (er.HResult == -2146233088)
                    return true;
                throw er;
            }
        }
        public bool Exists(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = Context.ExecuteAsync(operation).NoContext().GetAwaiter().GetResult();
            if (result.Result == null)
                return false;
            RystemCache cached = (RystemCache)result.Result;
            if (DateTime.UtcNow.Ticks > cached.E)
            {
                this.Delete(key);
                return false;
            }
            return true;
        }
        public IEnumerable<string> List()
        {
            TableQuery tableQuery = new TableQuery
            {
                FilterString = $"PartitionKey eq '{FullName}'"
            };
            TableContinuationToken tableContinuationToken = new TableContinuationToken();
            List<string> keys = new List<string>();
            do
            {
                TableQuerySegment tableQuerySegment = Context.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken).NoContext().GetAwaiter().GetResult();
                IEnumerable<string> keysFromQuery = tableQuerySegment.Results.Select(x => x.RowKey);
                tableContinuationToken = tableQuerySegment.ContinuationToken;
                keys.AddRange(keysFromQuery);
            } while (tableContinuationToken != null);
            return keys;
        }
        private class RystemCache : TableEntity
        {
            public string Data { get; set; }
            public long E { get; set; }
        }
    }
}
