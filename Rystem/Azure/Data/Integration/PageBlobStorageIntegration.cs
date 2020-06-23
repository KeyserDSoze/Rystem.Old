using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Rystem.Data.Integration;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Data
{
    internal class PageBlobStorageIntegration<TEntity> : BlobStorageBaseIntegration<TEntity>, IDataIntegration<TEntity>
        where TEntity : IData
    {
        private protected override IDataReader<TEntity> DefaultReader => new CsvDataManager<TEntity>();
        private protected override IDataWriter<TEntity> DefaultWriter => new CsvDataManager<TEntity>();
        public PageBlobStorageIntegration(DataConfiguration configuration, TEntity entity) : base(configuration, entity)
        {
        }
        public async Task<bool> DeleteAsync(TEntity entity)
            => await this.DeleteAsync(entity.Name).NoContext();

        public async Task<bool> ExistsAsync(TEntity entity)
            => await this.ExistsAsync(entity.Name).NoContext();

        public Task<TEntity> FetchAsync(TEntity entity)
            => throw new NotImplementedException($"With pageblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");

        public async Task<IList<TEntity>> ListAsync(TEntity entity, string prefix, int? takeCount)
        {
            var client = context ?? await GetContextAsync().NoContext();
            List<TEntity> items = new List<TEntity>();
            CancellationToken token = default;
            int count = 0;
            await foreach (BlobItem blobItem in client.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix, token))
            {
                items.AddRange(await this.ReadAsync(blobItem).NoContext());
                count++;
                if (takeCount != null && items.Count >= takeCount)
                    break;
            }
            return items;
        }

        public async Task<IList<string>> SearchAsync(TEntity entity, string prefix, int? takeCount)
            => await this.SearchAsync(prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, string prefix, int? takeCount)
          => await this.FetchPropertiesAsync(prefix, takeCount).NoContext();

        private const long Size = 512;
        public async Task<bool> WriteAsync(TEntity entity, long offset)
        {
            var client = context ?? await GetContextAsync().NoContext();
            var pageBlob = client.GetPageBlobClient(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync(entity).NoContext();
            if (!await pageBlob.ExistsAsync().NoContext())
                await pageBlob.CreateAsync(Size);
            long sized = Size - dummy.Stream.Length;
            if (sized != 0)
            {
                byte[] baseMemoryStream = new BinaryReader(dummy.Stream).ReadBytes((int)dummy.Stream.Length);
                byte[] finalizingStream = new byte[Size];
                for (int i = 0; i < Size; i++)
                    finalizingStream[i] = i < dummy.Stream.Length ? baseMemoryStream[i] : (byte)0;
                await pageBlob.UploadPagesAsync(new MemoryStream(finalizingStream), Size * offset, null).NoContext();
            }
#warning È sicuramente buggato, perchè scrive solo quando è in un if buggato
            return true;
        }
    }
}
