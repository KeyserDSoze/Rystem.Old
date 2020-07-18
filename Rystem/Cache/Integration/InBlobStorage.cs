using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Rystem.Const;

namespace Rystem.Cache
{
    internal class InBlobStorage<T> : ICacheIntegrationAsync<T>
    {
        private readonly CacheConfiguration Properties;
        private protected BlobContainerClient context;
        private static readonly RaceCondition RaceCondition = new RaceCondition();
        private protected async Task<BlobContainerClient> GetContextAsync()
        {
            if (context == null)
                await RaceCondition.ExecuteAsync(async () =>
                {
                    if (context == null)
                    {
                        var client = new BlobServiceClient(Configuration.ConnectionString);
                        var preContext = client.GetBlobContainerClient(ContainerName.ToLower());
                        if (!await preContext.ExistsAsync().NoContext())
                            await preContext.CreateIfNotExistsAsync().NoContext();
                        context = preContext;
                    }
                }).NoContext();
            return context;
        }
        private static long ExpireCache = 0;
        private const string ContainerName = "rystemcache";
        private readonly string FullName;
        private readonly RystemCacheConfiguration Configuration;
        internal InBlobStorage(RystemCacheConfiguration configuration)
        {
            this.FullName = configuration.CloudProperties.Namespace + "/";
            Configuration = configuration;
            Properties = configuration.CloudProperties;
            ExpireCache = Properties.ExpireTimeSpan.Ticks;
        }
        public async Task<T> InstanceAsync(string key)
        {
            var client = context ?? await GetContextAsync();
            BlockBlobClient cloudBlob = client.GetBlockBlobClient(CloudKeyToString(key));
            Response<BlobProperties> properties = await cloudBlob.GetPropertiesAsync().NoContext();
            if (!string.IsNullOrWhiteSpace(properties.Value.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(properties.Value.CacheControl)))
            {
                await this.DeleteAsync(key).NoContext();
                return default;
            }
            else
            {
                using (StreamReader reader = new StreamReader((await cloudBlob.DownloadAsync().NoContext()).Value.Content))
                    return (await reader.ReadToEndAsync().NoContext()).FromDefaultJson<T>();
            }
        }
        public async Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime)
        {
            var client = context ?? await GetContextAsync();
            long expiring = ExpireCache;
            if (expiringTime != default)
                expiring = expiringTime.Ticks;
            BlockBlobClient cloudBlob = client.GetBlockBlobClient(CloudKeyToString(key));
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToDefaultJson())))
                await cloudBlob.UploadAsync(stream).NoContext();
            if (expiring > 0)
                await cloudBlob.SetHttpHeadersAsync(new BlobHttpHeaders() { CacheControl = (expiring + DateTime.UtcNow.Ticks).ToString() });
            return true;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            var client = context ?? await GetContextAsync();
            return await client.GetBlockBlobClient(CloudKeyToString(key)).DeleteIfExistsAsync().NoContext();
        }

        public async Task<MultitonStatus<T>> ExistsAsync(string key)
        {
            var client = context ?? await GetContextAsync();
            BlockBlobClient cloudBlob = client.GetBlockBlobClient(CloudKeyToString(key));
            if (await cloudBlob.ExistsAsync().NoContext())
            {
                if (ExpireCache > 0)
                {
                    Response<BlobProperties> properties = await cloudBlob.GetPropertiesAsync().NoContext();
                    if (!string.IsNullOrWhiteSpace(properties.Value.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(properties.Value.CacheControl)))
                    {
                        await this.DeleteAsync(key).NoContext();
                        return MultitonStatus<T>.NotOk();
                    }
                }
                return MultitonStatus<T>.Ok();
            }
            return MultitonStatus<T>.NotOk();
        }

        public async Task<IEnumerable<string>> ListAsync()
        {
            var client = context ?? await GetContextAsync();
            IList<string> items = new List<string>();
            await foreach (var t in client.GetBlobsAsync(BlobTraits.All, BlobStates.All, FullName))
                items.Add(t.Name.Replace(FullName, string.Empty));
            return items;
        }
        public Task WarmUp()
           => Task.CompletedTask;
        private string CloudKeyToString(string keyString)
           => $"{FullName}{keyString}";
    }
}
