using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.AggregatedData.Integration;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    internal class BlockBlobStorageIntegration<TEntity> : IAggregatedDataIntegration<TEntity>
        where TEntity : IAggregatedData
    {
        private CloudBlobContainer Context;
        private IAggregatedDataReader<TEntity> Reader;
        private IAggregatedDataWriter<TEntity> Writer;
        internal BlockBlobStorageIntegration(AggregatedDataConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.Reader = configuration.Reader ?? new JsonDataManager<TEntity>();
            this.Writer = configuration.Writer ?? new JsonDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.DeleteAsync(this.Context.GetBlockBlobReference(entity.Name));

        public async Task<bool> ExistsAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.ExistsAsync(this.Context.GetBlockBlobReference(entity.Name));

        public async Task<TEntity> FetchAsync(IAggregatedData entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            if (await cloudBlob.ExistsAsync())
                return await this.ReadAsync(cloudBlob);
            return default;
        }
        private async Task<TEntity> ReadAsync(ICloudBlob cloudBlob)
        {
            return this.Reader.Read(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
            });
        }

        public async Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
        {
            IList<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(await this.ReadAsync(blobItem as ICloudBlob));
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount);

        public async Task<bool> AppendAsync(IAggregatedData entity, long offset = 0)
        {
            await Task.Delay(0);
            throw new NotImplementedException($"Blockblob doesn't allow append. Try to use {AggregatedDataType.AppendBlob} or {AggregatedDataType.DataLakeV2}");
        }

        public async Task<string> WriteAsync(IAggregatedData entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            AggregatedDataDummy dummy = this.Writer.Write((TEntity)entity);
            await cloudBlob.UploadFromStreamAsync(dummy.Stream);
            string path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
            await BlobStorageBaseIntegration.SetBlobPropertyIfNecessary(entity, cloudBlob, dummy);
            return path;
        }
    }
}
