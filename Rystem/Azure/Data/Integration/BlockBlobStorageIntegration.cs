using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.Data.Integration;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal class BlockBlobStorageIntegration<TEntity> : BlobStorageBaseIntegration<TEntity>, IDataIntegration<TEntity>
        where TEntity : IData
    {
        internal BlockBlobStorageIntegration(DataConfiguration<TEntity> configuration, TEntity entity) : base(configuration, entity)
        {
            this.Reader = configuration.Reader ?? new JsonDataManager<TEntity>();
            this.Writer = configuration.Writer ?? new JsonDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(TEntity entity)
            => await this.DeleteAsync(this.Context.GetBlockBlobReference(entity.Name)).NoContext();

        public async Task<bool> ExistsAsync(TEntity entity)
            => await this.ExistsAsync(this.Context.GetBlockBlobReference(entity.Name)).NoContext();

        public async Task<TEntity> FetchAsync(TEntity entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            if (await cloudBlob.ExistsAsync().NoContext())
                return (await this.ReadAsync(cloudBlob).NoContext()).FirstOrDefault();
            return default;
        }

        public async Task<IList<TEntity>> ListAsync(TEntity entity, string prefix = null, int? takeCount = null)
        {
            IList<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, this.BlobRequestOptions, new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add((await this.ReadAsync(blobItem as ICloudBlob).NoContext()).FirstOrDefault());
                }
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(TEntity entity, string prefix = null, int? takeCount = null)
            => await this.SearchAsync(this.Context, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, string prefix, int? takeCount)
          => await this.FetchPropertiesAsync(this.Context, prefix, takeCount).NoContext();

        public async Task<bool> WriteAsync(TEntity entity, long offset)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync(entity).NoContext();
            await cloudBlob.UploadFromStreamAsync(dummy.Stream).NoContext();
            await this.SetBlobPropertyIfNecessaryAsync(entity, cloudBlob, dummy).NoContext();
            return true;
        }
    }
}
