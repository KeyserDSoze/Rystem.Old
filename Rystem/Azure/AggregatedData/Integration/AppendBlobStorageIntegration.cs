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
    internal class AppendBlobStorageIntegration<TEntity> : IAggregatedDataIntegration<TEntity>
        where TEntity : IAggregatedData
    {
        private CloudBlobContainer Context;
        private IAggregatedDataWriter<TEntity> Writer;
        private IAggregatedDataListReader<TEntity> ListReader;
        private const int MaximumAttempt = 3;
        internal AppendBlobStorageIntegration(AggregatedDataConfiguration<TEntity> configuration)
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
            throw new NotImplementedException($"With appendblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");
        }
        private async Task<IList<TEntity>> ReadAsync(ICloudBlob cloudBlob)
        {
            return this.ListReader.Read(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
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

        public async Task<bool> AppendAsync(IAggregatedData entity, long offset = 0)
        {
            int attempt = 0;
            CloudAppendBlob appendBlob = this.Context.GetAppendBlobReference(entity.Name);
            AggregatedDataDummy dummy = this.Writer.Write((TEntity)entity);
            do
            {
                try
                {
                    await appendBlob.AppendFromStreamAsync(dummy.Stream);
                    //attempt = configuration.MaximumAttempt;
                    break;
                }
                catch (AggregateException aggregateException)
                {
                    await Task.Delay(20);
                    if (attempt >= MaximumAttempt)
                        //if (attempt >= configuration.MaximumAttempt)
                        throw aggregateException;
                }
                catch (Exception er)
                {
                    if (er.Message == "The specified blob does not exist.")
                        await appendBlob.CreateOrReplaceAsync();
                    else if (er.HResult == -2146233088)
                    {
                        //when the blob has 50000 block append
                        throw er;
                    }
                    else
                        throw er;
                }
                attempt++;
                //} while (attempt <= configuration.MaximumAttempt);
            } while (attempt <= MaximumAttempt);
            return attempt <= MaximumAttempt;
        }

        public async Task<string> WriteAsync(IAggregatedData entity) 
            => throw new NotImplementedException($"You must use Append with {AggregatedDataType.AppendBlob}");
    }
}
