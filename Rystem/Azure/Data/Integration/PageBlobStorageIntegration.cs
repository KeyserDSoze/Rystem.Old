using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.Data.Integration;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal class PageBlobStorageIntegration<TEntity> : IDataIntegration<TEntity>
        where TEntity : IData
    {
        private readonly CloudBlobContainer Context;
        private readonly IDataWriter<TEntity> Writer;
        private readonly IDataReader<TEntity> ListReader;
        internal PageBlobStorageIntegration(DataConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ToResult();
            this.Writer = configuration.Writer ?? new CsvDataManager<TEntity>();
            this.ListReader = configuration.Reader ?? new CsvDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(IData entity)
            => await BlobStorageBaseIntegration.DeleteAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public async Task<bool> ExistsAsync(IData entity)
            => await BlobStorageBaseIntegration.ExistsAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public Task<TEntity> FetchAsync(IData entity)
            => throw new NotImplementedException($"With pageblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");
        private async Task<IList<TEntity>> ReadAsync(ICloudBlob cloudBlob)
        {
            return (await this.ListReader.ReadAsync(new DataWrapper()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob).NoContext(),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
            }).NoContext()).Entities;
        }

        public async Task<IList<TEntity>> ListAsync(IData entity, string prefix, int? takeCount)
        {
            List<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, BlobStorageBaseIntegration.BlobRequestOptions, new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.AddRange(await this.ReadAsync(blobItem as ICloudBlob).NoContext());
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(IData entity, string prefix, int? takeCount)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(IData entity, string prefix, int? takeCount)
          => await BlobStorageBaseIntegration.FetchPropertiesAsync(this.Context, prefix, takeCount).NoContext();

        private readonly static object TrafficLight = new object();
        private const long Size = 512;
        public async Task<bool> WriteAsync(IData entity, long offset)
        {
            CloudPageBlob pageBlob = this.Context.GetPageBlobReference(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync((TEntity)entity).NoContext();
            if (!await pageBlob.ExistsAsync().NoContext())
                lock (TrafficLight)
                    if (!pageBlob.ExistsAsync().ToResult())
                        pageBlob.CreateAsync(Size).ToResult();
            long sized = Size - dummy.Stream.Length;
            if (sized != 0)
            {
                byte[] baseMemoryStream = new BinaryReader(dummy.Stream).ReadBytes((int)dummy.Stream.Length);
                byte[] finalizingStream = new byte[Size];
                for (int i = 0; i < Size; i++)
                    finalizingStream[i] = i < dummy.Stream.Length ? baseMemoryStream[i] : (byte)0;
                await pageBlob.WritePagesAsync(new MemoryStream(finalizingStream), Size * offset, null).NoContext();
            }
#warning È sicuramente buggato, perchè scrive solo quando è in un if buggato
            return true;
        }
    }
}
