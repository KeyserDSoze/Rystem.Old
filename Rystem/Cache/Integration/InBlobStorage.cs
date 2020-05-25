using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Const;

namespace Rystem.Cache
{
    internal class InBlobStorage<T> : IMultitonIntegrationAsync<T>
    {
        private readonly CacheProperties Properties;
        private static readonly object TrafficLight = new object();
        private CloudBlobContainer context;
        private protected CloudBlobContainer Context
        {
            get
            {
                if (context != null)
                    return context;
                lock (TrafficLight)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration.ConnectionString);
                    CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
                    context = Client.GetContainerReference(ContainerName);
                }
                context.CreateIfNotExistsAsync().ToResult();
                return context;
            }
        }
        private static long ExpireCache = 0;
        private const string ContainerName = "rystemcache";
        private readonly static string FullName = typeof(T).FullName + "/";
        private readonly RystemCacheProperty Configuration;
        internal InBlobStorage(RystemCacheProperty configuration)
        {
            Configuration = configuration;
            Properties = configuration.CloudProperties;
            ExpireCache = Properties.ExpireTimeSpan.Ticks;
        }
        public async Task<T> InstanceAsync(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            await cloudBlob.FetchAttributesAsync().NoContext();
            if (!string.IsNullOrWhiteSpace(cloudBlob.Properties.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(cloudBlob.Properties.CacheControl)))
            {
                await this.DeleteAsync(key).NoContext();
                return default;
            }
            else
            {
                using (StreamReader reader = new StreamReader(cloudBlob.OpenReadAsync(null, null, null).ToResult()))
                    return (await reader.ReadToEndAsync().NoContext()).FromDefaultJson<T>();
            }
        }
        public async Task<bool> UpdateAsync(string key, T value, TimeSpan expiringTime)
        {
            long expiring = ExpireCache;
            if (expiringTime != default)
                expiring = expiringTime.Ticks;
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            if (expiring > 0)
                cloudBlob.Properties.CacheControl = (expiring + DateTime.UtcNow.Ticks).ToString();
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToDefaultJson())))
                await cloudBlob.UploadFromStreamAsync(stream).NoContext();
            return true;
        }

        public async Task<bool> DeleteAsync(string key)
            => await Context.GetBlockBlobReference(CloudKeyToString(key)).DeleteIfExistsAsync().NoContext();
        public async Task<MultitonStatus<T>> ExistsAsync(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            if (await cloudBlob.ExistsAsync().NoContext())
            {
                if (ExpireCache > 0)
                {
                    cloudBlob.FetchAttributesAsync().ToResult();
                    if (!string.IsNullOrWhiteSpace(cloudBlob.Properties.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(cloudBlob.Properties.CacheControl)))
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
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await Context.ListBlobsSegmentedAsync(FullName, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(((ICloudBlob)blobItem).Name.Replace(FullName, ""));
                }
            } while (token != null);
            return items;
        }
        public Task WarmUp()
           => Task.CompletedTask;
        private static string CloudKeyToString(string keyString)
           => $"{FullName}{keyString}";
    }
}
