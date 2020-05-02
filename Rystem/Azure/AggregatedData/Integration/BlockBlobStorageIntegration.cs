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
        where TEntity : IAggregatedData, new()
    {
        private readonly CloudBlobContainer Context;
        private readonly IAggregatedDataReader<TEntity> Reader;
        private readonly IAggregatedDataWriter<TEntity> Writer;
        internal BlockBlobStorageIntegration(AggregatedDataConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().NoContext().GetAwaiter().GetResult();
            this.Reader = configuration.Reader ?? new JsonDataManager<TEntity>();
            this.Writer = configuration.Writer ?? new JsonDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.DeleteAsync(this.Context.GetBlockBlobReference(entity.Name)).NoContext();

        public async Task<bool> ExistsAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.ExistsAsync(this.Context.GetBlockBlobReference(entity.Name)).NoContext();

        public async Task<TEntity> FetchAsync(IAggregatedData entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            if (await cloudBlob.ExistsAsync().NoContext())
                return await this.ReadAsync(cloudBlob).NoContext();
            return default;
        }
        private async Task<TEntity> ReadAsync(ICloudBlob cloudBlob)
        {
            return await this.Reader.ReadAsync(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob).NoContext(),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
            }).NoContext();
        }

        public async Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
        {
            IList<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, BlobStorageBaseIntegration.BlobRequestOptions, new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(await this.ReadAsync(blobItem as ICloudBlob).NoContext());
                }
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount).NoContext();
        public async Task<IList<AggregatedDataDummy>> FetchPropertiesAsync(IAggregatedData entity, string prefix, int? takeCount)
          => await BlobStorageBaseIntegration.FetchPropertiesAsync(this.Context, prefix, takeCount).NoContext();

        public async Task<bool> WriteAsync(IAggregatedData entity, long offset)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            AggregatedDataDummy dummy = await this.Writer.WriteAsync((TEntity)entity).NoContext();
            await cloudBlob.UploadFromStreamAsync(dummy.Stream).NoContext();
            await BlobStorageBaseIntegration.SetBlobPropertyIfNecessaryAsync(entity, cloudBlob, dummy).NoContext();
            return true;
        }
    }
}
