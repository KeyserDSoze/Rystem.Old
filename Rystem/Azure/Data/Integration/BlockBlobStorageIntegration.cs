using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Rystem.Data.Integration;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Data
{
    internal class BlockBlobStorageIntegration<TEntity> : BlobStorageBaseIntegration<TEntity>, IDataIntegration<TEntity>
        where TEntity : IData
    {
        private protected override IDataReader<TEntity> DefaultReader => new JsonDataManager<TEntity>();
        private protected override IDataWriter<TEntity> DefaultWriter => new JsonDataManager<TEntity>();
        internal BlockBlobStorageIntegration(DataConfiguration configuration, TEntity entity) : base(configuration, entity)
        {
        }
        public async Task<bool> DeleteAsync(TEntity entity)
            => await this.DeleteAsync(entity.Name).NoContext();

        public async Task<bool> ExistsAsync(TEntity entity)
            => await this.ExistsAsync(entity.Name).NoContext();

        public async Task<TEntity> FetchAsync(TEntity entity)
        {
            BlockBlobClient cloudBlob = this.Context.GetBlockBlobClient(entity.Name);
            if (await cloudBlob.ExistsAsync().NoContext())
                return (await this.ReadAsync(cloudBlob).NoContext()).FirstOrDefault();
            return default;
        }

        public async Task<IList<TEntity>> ListAsync(TEntity entity, string prefix = null, int? takeCount = null)
        {
            List<TEntity> items = new List<TEntity>();
            int count = 0;
            await foreach (BlobItem blobItem in Context.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix))
            {
                items.AddRange(await this.ReadAsync(blobItem).NoContext());
                count++;
                if (takeCount != null && items.Count >= takeCount)
                    break;
            }
            return items;
        }
        public async Task<IList<string>> SearchAsync(TEntity entity, string prefix = null, int? takeCount = null)
            => await this.SearchAsync(prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, string prefix, int? takeCount)
          => await this.FetchPropertiesAsync(prefix, takeCount).NoContext();
        public async Task<bool> WriteAsync(TEntity entity, long offset)
        {
            BlockBlobClient cloudBlob = this.Context.GetBlockBlobClient(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync(entity).NoContext();
            await cloudBlob.UploadAsync(dummy.Stream).NoContext();
            await this.SetBlobProperty(entity, cloudBlob, dummy).NoContext();
            return true;
        }
    }
}