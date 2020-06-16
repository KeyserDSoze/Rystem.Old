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
    internal class AppendBlobStorageIntegration<TEntity> : BlobStorageBaseIntegration<TEntity>, IDataIntegration<TEntity>
        where TEntity : IData
    {
        private const int MaximumAttempt = 3;
        private protected override IDataReader<TEntity> DefaultReader => new CsvDataManager<TEntity>();
        private protected override IDataWriter<TEntity> DefaultWriter => new CsvDataManager<TEntity>();
        internal AppendBlobStorageIntegration(DataConfiguration configuration, TEntity entity) : base(configuration, entity)
        {
        }
        public async Task<bool> DeleteAsync(TEntity entity)
            => await this.DeleteAsync(entity.Name).NoContext();

        public async Task<bool> ExistsAsync(TEntity entity)
            => await this.ExistsAsync(entity.Name).NoContext();

        public Task<TEntity> FetchAsync(TEntity entity)
            => throw new NotImplementedException($"With appendblob you can retrieve only a list of your items. Please use {nameof(ListAsync)}");

        public async Task<IList<TEntity>> ListAsync(TEntity entity, string prefix, int? takeCount)
        {
            List<TEntity> items = new List<TEntity>();
            CancellationToken token = default;
            int count = 0;
            await foreach (BlobItem blobItem in Context.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix, token))
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
        private const string BlobDoesntExist = "The specified blob does not exist.";
        private const string BlobNotFound = "(404) Not Found";
        public async Task<bool> WriteAsync(TEntity entity, long offset)
        {
            int attempt = 0;
            AppendBlobClient appendBlob = this.Context.GetAppendBlobClient(entity.Name);
            DataWrapper dummy = await this.Writer.WriteAsync(entity).NoContext();
            do
            {
                try
                {
                    await appendBlob.AppendBlockAsync(dummy.Stream).NoContext();
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
                        await appendBlob.CreateIfNotExistsAsync().NoContext();
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