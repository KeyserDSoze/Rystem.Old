using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.DataLake
{
    internal class BlockBlobStorageIntegration<TEntity> : IDataLakeIntegration
        where TEntity : IDataLake
    {
        private CloudBlobContainer Context;
        private IDataLakeReader<TEntity> Reader;
        private IDataLakeWriter Writer;
        internal BlockBlobStorageIntegration(DataLakeConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.Reader = configuration.Reader;
            this.Writer = configuration.Writer;
        }
        public async Task<bool> DeleteAsync(IDataLake entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            return await cloudBlob.DeleteIfExistsAsync();
        }

        public async Task<bool> ExistsAsync(IDataLake entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            return await cloudBlob.ExistsAsync();
        }

        public async Task<IDataLake> FetchAsync(IDataLake entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            if (await cloudBlob.ExistsAsync())
            {
                await cloudBlob.FetchAttributesAsync();
                var fileLength = cloudBlob.Properties.Length;
                byte[] fileByte = new byte[fileLength];
                await cloudBlob.DownloadToByteArrayAsync(fileByte, 0);
                return this.Reader.Read(new DataLakeDummy()
                {
                    Name = cloudBlob.Name,
#warning Aggiungere anche tutte le altre proprietà in set nel costruttore
                    Properties = new LakeProperties()
                    {
                        CacheControl = cloudBlob.Properties.CacheControl,
                        ContentDisposition = cloudBlob.Properties.ContentDisposition,
                        ContentEncoding = cloudBlob.Properties.ContentEncoding,
                        ContentLanguage = cloudBlob.Properties.ContentLanguage,
                        ContentMD5 = cloudBlob.Properties.ContentMD5,
                        ContentType = cloudBlob.Properties.ContentType,
                    },
                    Stream = new MemoryStream(fileByte)
                });
            }
            return null;
        }

        public async Task<IList<IDataLake>> ListAsync(IDataLake entity, string prefix = null, int? takeCount = null)
        {
            IList<IDataLake> items = new List<IDataLake>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(await this.FetchAsync(entity));
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(IDataLake entity, string prefix = null, int? takeCount = null)
        {
            IList<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (var blobItem in segment.Results)
                    items.Add(blobItem.StorageUri.PrimaryUri.ToString());
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        public async Task<bool> AppendAsync(IDataLake entity)
        {
            await Task.Delay(0);
            throw new NotImplementedException($"Blockblob doesn't allow append. Try to use {DataLakeType.AppendBlob} or {DataLakeType.DataLakeV2}");
        }

        public async Task<string> WriteAsync(IDataLake entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            DataLakeDummy dummy = this.Writer.Write(entity);
            await cloudBlob.UploadFromStreamAsync(dummy.Stream);
            string path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
            if (CheckBlobProperty())
                await cloudBlob.SetPropertiesAsync();
            return path;
            bool CheckBlobProperty()
            {
                bool changeSomethingInProperty = false;
                if (dummy.Properties != null)
                {
                    if (dummy.Properties.ContentType != cloudBlob.Properties.ContentType)
                    {
                        cloudBlob.Properties.ContentType = dummy.Properties.ContentType ?? MimeMapping.GetMimeMapping(entity.Name);
                        changeSomethingInProperty = true;
                    }
                    if (dummy.Properties.CacheControl != null && dummy.Properties.CacheControl != cloudBlob.Properties.CacheControl)
                    {
                        cloudBlob.Properties.CacheControl = dummy.Properties.CacheControl;
                        changeSomethingInProperty = true;
                    }
                    if (dummy.Properties.ContentDisposition != null && dummy.Properties.ContentDisposition != cloudBlob.Properties.ContentDisposition)
                    {
                        cloudBlob.Properties.ContentDisposition = dummy.Properties.ContentDisposition;
                        changeSomethingInProperty = true;
                    }
                    if (dummy.Properties.ContentEncoding != null && dummy.Properties.ContentEncoding != cloudBlob.Properties.ContentEncoding)
                    {
                        cloudBlob.Properties.ContentEncoding = dummy.Properties.ContentEncoding;
                        changeSomethingInProperty = true;
                    }
                    if (dummy.Properties.ContentLanguage != null && dummy.Properties.ContentLanguage != cloudBlob.Properties.ContentLanguage)
                    {
                        cloudBlob.Properties.ContentLanguage = dummy.Properties.ContentLanguage;
                        changeSomethingInProperty = true;
                    }
                    if (dummy.Properties.ContentMD5 != null && dummy.Properties.ContentMD5 != cloudBlob.Properties.ContentMD5)
                    {
                        cloudBlob.Properties.ContentMD5 = dummy.Properties.ContentMD5;
                        changeSomethingInProperty = true;
                    }
                }
                return changeSomethingInProperty;
            }
        }
    }
}
