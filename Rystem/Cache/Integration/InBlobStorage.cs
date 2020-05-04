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
    internal class InBlobStorage<T> : IMultitonIntegration<T>
        where T : IMultiton, new()
    {
        private static CloudBlobContainer Context;
        private static long ExpireCache = 0;
        private const string ContainerName = "rystemcache";
        private readonly static string FullName = typeof(T).FullName + "/";
        internal InBlobStorage(InCloudMultitonProperties configuration)
        {
            ExpireCache = configuration.ExpireTimeSpan.Ticks;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            Context = Client.GetContainerReference(ContainerName);
            Context.CreateIfNotExistsAsync().ToResult();
        }
        public T Instance(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            cloudBlob.FetchAttributesAsync().ToResult();
            if (!string.IsNullOrWhiteSpace(cloudBlob.Properties.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(cloudBlob.Properties.CacheControl)))
            {
                this.Delete(key);
                return default;
            }
            else
            {
                using (StreamReader reader = new StreamReader(cloudBlob.OpenReadAsync(null, null, null).ToResult()))
                    return reader.ReadToEnd().FromDefaultJson<T>();
            }
        }
        public bool Update(string key, T value, TimeSpan expiringTime)
        {
            long expiring = ExpireCache;
            if (expiringTime != default)
                expiring = expiringTime.Ticks;
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            if (expiring > 0)
                cloudBlob.Properties.CacheControl = (expiring + DateTime.UtcNow.Ticks).ToString();
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToDefaultJson())))
                cloudBlob.UploadFromStreamAsync(stream).ToResult();
            return true;
        }

        public bool Delete(string key)
            => Context.GetBlockBlobReference(CloudKeyToString(key)).DeleteIfExistsAsync().ToResult();
        public bool Exists(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            if (cloudBlob.ExistsAsync().ToResult())
            {
                if (ExpireCache > 0)
                {
                    cloudBlob.FetchAttributesAsync().ToResult();
                    if (!string.IsNullOrWhiteSpace(cloudBlob.Properties.CacheControl) && DateTime.UtcNow > new DateTime(long.Parse(cloudBlob.Properties.CacheControl)))
                    {
                        this.Delete(key);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public IEnumerable<string> List()
        {
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = Context.ListBlobsSegmentedAsync(FullName, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { }).ToResult();
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
        private static string CloudKeyToString(string keyString)
           => $"{FullName}{keyString}";
    }
}
