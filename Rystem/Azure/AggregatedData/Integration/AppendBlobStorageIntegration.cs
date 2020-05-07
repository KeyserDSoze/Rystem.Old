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
        private readonly CloudBlobContainer Context;
        private readonly IAggregatedDataWriter<TEntity> Writer;
        private readonly IAggregatedDataListReader<TEntity> ListReader;
        private const int MaximumAttempt = 3;
        internal AppendBlobStorageIntegration(AggregatedDataConfiguration<TEntity> configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ToResult();
            this.Writer = configuration.Writer ?? new CsvDataManager<TEntity>();
            this.ListReader = configuration.ListReader ?? new CsvDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.DeleteAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public async Task<bool> ExistsAsync(IAggregatedData entity)
            => await BlobStorageBaseIntegration.ExistsAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public Task<TEntity> FetchAsync(IAggregatedData entity)
            => throw new NotImplementedException($"With appendblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");
        private async Task<IList<TEntity>> ReadAsync(ICloudBlob cloudBlob)
        {
            return await this.ListReader.ReadAsync(new AggregatedDataDummy()
            {
                Name = cloudBlob.Name,
                Stream = await BlobStorageBaseIntegration.ReadAsync(cloudBlob).NoContext(),
                Properties = cloudBlob.Properties.ToAggregatedDataProperties()
            }).NoContext();
        }

        public async Task<IList<TEntity>> ListAsync(IAggregatedData entity, string prefix, int? takeCount)
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

        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix, int? takeCount)
            => await BlobStorageBaseIntegration.SearchAsync(this.Context, prefix, takeCount).NoContext();
        public async Task<IList<AggregatedDataDummy>> FetchPropertiesAsync(IAggregatedData entity, string prefix, int? takeCount)
            => await BlobStorageBaseIntegration.FetchPropertiesAsync(this.Context, prefix, takeCount).NoContext();
        private const string BlobDoesntExist = "The specified blob does not exist.";
        private const string BlobNotFound = "(404) Not Found";
        public async Task<bool> WriteAsync(IAggregatedData entity, long offset)
        {
            int attempt = 0;
            CloudAppendBlob appendBlob = this.Context.GetAppendBlobReference(entity.Name);
            AggregatedDataDummy dummy = await this.Writer.WriteAsync((TEntity)entity).NoContext();
            do
            {
                try
                {
                    await appendBlob.AppendFromStreamAsync(dummy.Stream).NoContext();
                    //attempt = configuration.MaximumAttempt;
                    break;
                }
                catch (AggregateException aggregateException)
                {
                    await Task.Delay(20).NoContext();
                    if (attempt >= MaximumAttempt)
                        //if (attempt >= configuration.MaximumAttempt)
                        throw aggregateException;
                }
                catch (Exception er)
                {
                    if (er.Message == BlobDoesntExist || er.Message.Contains(BlobNotFound))
                        await appendBlob.CreateOrReplaceAsync().NoContext();
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
    }
}
