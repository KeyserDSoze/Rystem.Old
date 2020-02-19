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
    internal class PageBlobStorageIntegration<TEntity> : IAggregatedDataIntegration<TEntity>
        where TEntity : IAggregatedData
    {
        private CloudBlobContainer Context;
        private IAggregatedDataWriter<TEntity> Writer;
        private IAggregatedDataListReader<TEntity> ListReader;
        internal PageBlobStorageIntegration(AggregatedDataConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.Writer = configuration.Writer ?? new CsvDataManager<TEntity>();
            this.ListReader = configuration.ListReader ?? new CsvDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.DeleteAsync(this.Context.GetAppendBlobReference(entity.Name));

        public async Task<bool> ExistsAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.ExistsAsync(this.Context.GetAppendBlobReference(entity.Name));

        public async Task<TEntity> FetchAsync(IAggregatedData entity)
        {
            await Task.Delay(0);
            throw new NotImplementedException($"With pageblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");
        }
        private async Task<IList<TEntity>> ReadAsync(ICloudBlob cloudBlob)
        {
            return await this.ListReader.ReadAsync(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
            });
        }

        public async Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix, int? takeCount)
        {
            List<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, BlobStorageBaseIntegration.BlobRequestOptions, new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.AddRange(await this.ReadAsync(blobItem as ICloudBlob));
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null);
            return items;
        }

        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix, int? takeCount)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount);
        public async Task<IList<AggregatedDataDummy>> FetchPropertiesAsync(IAggregatedData entity, string prefix, int? takeCount)
          => await BlobStorageBaseIntegration.FetchPropertiesAsync(this.Context, prefix, takeCount);

        private static object TrafficLight = new object();
        private const long Size = 512;
        public async Task<bool> WriteAsync(IAggregatedData entity, long offset)
        {
            CloudPageBlob pageBlob = this.Context.GetPageBlobReference(entity.Name);
            AggregatedDataDummy dummy = await this.Writer.WriteAsync((TEntity)entity);
            if (!await pageBlob.ExistsAsync())
                lock (TrafficLight)
                    if (!pageBlob.ExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                        pageBlob.CreateAsync(Size).ConfigureAwait(false).GetAwaiter().GetResult();
            long sized = Size - dummy.Stream.Length;
            if (sized != 0)
            {
                byte[] baseMemoryStream = new BinaryReader(dummy.Stream).ReadBytes((int)dummy.Stream.Length);
                byte[] finalizingStream = new byte[Size];
                for (int i = 0; i < Size; i++)
                    finalizingStream[i] = i < dummy.Stream.Length ? baseMemoryStream[i] : (byte)0;
                await pageBlob.WritePagesAsync(new MemoryStream(finalizingStream), Size * offset, null);
            }
#warning È sicuramente buggato, perchè scrive solo quando è in un if buggato
            return true;
        }
    }
}
