using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Rystem.Cache;

namespace Rystem.Cache
{
    internal class InBlobStorage<T> : AMultitonIntegration<T>
        where T : IMultiton
    {
        private static CloudBlobContainer Context;
        private static int ExpireCache = 0;
        private const string ContainerName = "rystemcache";
        private readonly static string FullName = typeof(T).FullName;
        internal InBlobStorage(MultitonInstaller.MultitonConfiguration configuration)
        {
            ExpireCache = configuration.ExpireCache;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            Context = Client.GetContainerReference(ContainerName);
            Context.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }
        internal override T Instance(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            cloudBlob.FetchAttributesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var fileLength = cloudBlob.Properties.Length;
            byte[] fileByte = new byte[fileLength];
            cloudBlob.DownloadToByteArrayAsync(fileByte, 0);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(fileByte), MultitonConst.JsonSettings);
        }
        internal override bool Update(string key, T value)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, MultitonConst.JsonSettings))))
                cloudBlob.UploadFromStreamAsync(stream).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }

        internal override bool Delete(string key) => Context.GetBlockBlobReference(CloudKeyToString(key)).DeleteIfExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        internal override bool Exists(string key)
        {
            ICloudBlob cloudBlob = Context.GetBlockBlobReference(CloudKeyToString(key));
            if (cloudBlob.ExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                cloudBlob.FetchAttributesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (DateTime.UtcNow.Ticks - cloudBlob.Properties.LastModified.Value.ToUniversalTime().Ticks <= TimeSpan.FromMinutes(ExpireCache).Ticks)
                    return false;
                return true;
            }
            return false;
        }

        internal override IEnumerable<string> List()
        {
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = Context.ListBlobsSegmentedAsync(FullName, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { }).ConfigureAwait(false).GetAwaiter().GetResult();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(((ICloudBlob)blobItem).Name);
                }
            } while (token != null);
            return items;
        }
        private static string CloudKeyToString(string keyString)
           => $"{FullName}/{keyString}";
    }
}
