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
        private IDataLakeWriter Writer;
        private IAggregatedDataListReader<TEntity> ListReader;
        internal PageBlobStorageIntegration(DataAggregationConfiguration<TEntity> configuration)
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
            return this.ListReader.Read(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = new MemoryStream(await BlobStorageBaseIntegration.ReadAsync(cloudBlob)),
#warning Aggiungere anche tutte le altre proprietà in set nel costruttore
                Properties = new AggregatedDataProperties()
                {
                    CacheControl = cloudBlob.Properties.CacheControl,
                    ContentDisposition = cloudBlob.Properties.ContentDisposition,
                    ContentEncoding = cloudBlob.Properties.ContentEncoding,
                    ContentLanguage = cloudBlob.Properties.ContentLanguage,
                    ContentMD5 = cloudBlob.Properties.ContentMD5,
                    ContentType = cloudBlob.Properties.ContentType,
                }
            });
        }

        public async Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
        {
            List<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
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

        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount);

        private static object TrafficLight = new object();
        private const long Size = 512;
        public async Task<bool> AppendAsync(IAggregatedData entity, long offset = 0)
        {
            CloudPageBlob pageBlob = this.Context.GetPageBlobReference(entity.Name);
            AggregatedDataDummy dummy = this.Writer.Write(entity);
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

        public async Task<string> WriteAsync(IAggregatedData entity)
        {
            ICloudBlob cloudBlob = this.Context.GetBlockBlobReference(entity.Name);
            AggregatedDataDummy dummy = this.Writer.Write(entity);
            await cloudBlob.UploadFromStreamAsync(dummy.Stream);
            string path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
            await BlobStorageBaseIntegration.SetBlobPropertyIfNecessary(entity, cloudBlob, dummy);
            return path;
        }
    }
}
