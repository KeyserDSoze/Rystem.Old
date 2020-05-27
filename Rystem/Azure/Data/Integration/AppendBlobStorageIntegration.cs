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
    internal class AppendBlobStorageIntegration<TEntity> : BlobStorageBaseIntegration<TEntity>, IDataIntegration<TEntity>
        where TEntity : IData
    {
        private const int MaximumAttempt = 3;
        internal AppendBlobStorageIntegration(DataConfiguration configuration, TEntity entity) : base(configuration, entity)
        {
            this.Writer = configuration.Writer as IDataWriter<TEntity> ?? new CsvDataManager<TEntity>();
            this.Reader = configuration.Reader as IDataReader<TEntity> ?? new CsvDataManager<TEntity>();
        }
        public async Task<bool> DeleteAsync(TEntity entity)
            => await this.DeleteAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public async Task<bool> ExistsAsync(TEntity entity)
            => await this.ExistsAsync(this.Context.GetAppendBlobReference(entity.Name)).NoContext();

        public Task<TEntity> FetchAsync(TEntity entity)
            => throw new NotImplementedException($"With appendblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");

        public async Task<IList<TEntity>> ListAsync(TEntity entity, string prefix, int? takeCount)
        {
            List<TEntity> items = new List<TEntity>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await this.Context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, this.BlobRequestOptions, new OperationContext() { }).NoContext();
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

        public async Task<IList<string>> SearchAsync(TEntity entity, string prefix, int? takeCount)
            => await this.SearchAsync(this.Context, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, string prefix, int? takeCount)
            => await this.FetchPropertiesAsync(this.Context, prefix, takeCount).NoContext();
        private const string BlobDoesntExist = "The specified blob does not exist.";
        private const string BlobNotFound = "(404) Not Found";
        public async Task<bool> WriteAsync(TEntity entity, long offset)
        {
            int attempt = 0;
            CloudAppendBlob appendBlob = this.Context.GetAppendBlobReference(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync(entity).NoContext();
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
