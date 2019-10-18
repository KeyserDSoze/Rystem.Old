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
        private static long ExpireCache = 0;
        private const string TableName = "RystemCache";
        private readonly static string FullName = typeof(T).FullName;
        internal InTableStorage(InCloudMultitonProperties configuration)
        {
            ExpireCache = configuration.ExpireTimeSpan.Ticks;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Context = tableClient.GetTableReference(TableName);
            Context.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }
        internal override T Instance(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return result.Result != null ? JsonConvert.DeserializeObject<T>(((RystemCache)result.Result).Data, MultitonConst.JsonSettings) : default(T);
        }
        internal override bool Update(string key, T value)
        {
            RystemCache rystemCache = new RystemCache()
            {
                PartitionKey = FullName,
                RowKey = key,
                Data = JsonConvert.SerializeObject(value, MultitonConst.JsonSettings)
            };
            TableOperation operation = TableOperation.InsertOrReplace(rystemCache);
            TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            return (esito.HttpStatusCode == 204);
        }
        internal override bool Delete(string key)
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
                TableResult esito = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
                return (esito.HttpStatusCode == 204);
            }
            catch (StorageException er)
            {
                if (er.HResult == -2146233088)
                    return true;
                throw er;
            }
        }
        internal override bool Exists(string key)
        {
            TableOperation operation = TableOperation.Retrieve<RystemCache>(FullName, key);
            TableResult result = Context.ExecuteAsync(operation).GetAwaiter().GetResult();
            if (result.Result == null)
                return false;
            RystemCache cached = (RystemCache)result.Result;
            if (ExpireCache > 0 && DateTime.UtcNow.Ticks - cached.Timestamp.UtcDateTime.Ticks > ExpireCache)
                return false;
            return true;
        }
        internal override IEnumerable<string> List()
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
        private class RystemCache : TableEntity
        {
            public string Data { get; set; }
        }
    }
}
