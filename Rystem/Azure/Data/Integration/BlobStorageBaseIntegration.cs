using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading;
using Azure;
using System.Data.Common;
using Azure.Storage.Blobs.Specialized;

namespace Rystem.Data.Integration
{
    internal abstract class BlobStorageBaseIntegration<TEntity>
        where TEntity : IData
    {
        private protected abstract IDataReader<TEntity> DefaultReader { get; }
        private protected abstract IDataWriter<TEntity> DefaultWriter { get; }
        private static readonly object TrafficLight = new object();
        private BlobContainerClient context;
        private protected BlobContainerClient Context
        {
            get
            {
                if (context != null)
                    return context;
                lock (TrafficLight)
                {
                    if (context != null)
                        return context;
                    var client = new BlobServiceClient(Configuration.ConnectionString);
                    context = client.GetBlobContainerClient(Configuration.Name.ToLower());
                }
                if (!context.Exists())
                    context.CreateIfNotExistsAsync().ToResult();
                return context;
            }
        }
        private protected IDataWriter<TEntity> Writer;
        private protected IDataReader<TEntity> Reader;
        private protected readonly DataConfiguration Configuration;
        private protected readonly Type EntityType;
        internal BlobStorageBaseIntegration(DataConfiguration configuration, TEntity entity)
        {
            this.Configuration = configuration;
            this.EntityType = entity.GetType();
            if (configuration.Reader != null && !(configuration.Reader is IDataReader<TEntity>))
                throw new MissingMethodException($"Installed reader {configuration.Reader.GetType().FullName} is not a {typeof(IDataReader<TEntity>).FullName}");
            if (configuration.Writer != null && !(configuration.Writer is IDataWriter<TEntity>))
                throw new MissingMethodException($"Installed writer {configuration.Writer.GetType().FullName} is not a {typeof(IDataWriter<TEntity>).FullName}");
            this.Reader = configuration.Reader as IDataReader<TEntity> ?? DefaultReader;
            this.Writer = configuration.Writer as IDataWriter<TEntity> ?? DefaultWriter;
        }
        private protected async Task<bool> DeleteAsync(string name)
            => await Context.GetBlobClient(name).DeleteIfExistsAsync().NoContext();
        private protected async Task<bool> ExistsAsync(string name)
            => await Context.GetBlobClient(name).ExistsAsync().NoContext();
        private protected async Task<IList<string>> SearchAsync(string prefix = null, int? takeCount = null)
        {
            IList<string> items = new List<string>();
            CancellationToken token = default;
            int count = 0;
            await foreach (var t in Context.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix, token))
            {
                items.Add(t.Name);
                count++;
                if (takeCount != null && items.Count >= takeCount)
                    break;
            }
            return items;
        }
        private protected async Task<IList<DataWrapper>> FetchPropertiesAsync(string prefix = null, int? takeCount = null)
        {
            IList<DataWrapper> items = new List<DataWrapper>();
            CancellationToken token = default;
            int count = 0;
            await foreach (var t in Context.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix, token))
            {
                items.Add(new DataWrapper() { Name = t.Name, Properties = t.Properties.ToDataProperties(t.Metadata) });
                count++;
                if (takeCount != null && items.Count >= takeCount)
                    break;
            }
            return items;
        }
        private protected async Task<bool> SetBlobProperty(IData entity, BlobBaseClient cloudBlob, DataWrapper wrapper)
        {
            BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders()
            {
                CacheControl = entity.Properties.CacheControl,
                ContentDisposition = entity.Properties.ContentDisposition,
                ContentEncoding = entity.Properties.ContentEncoding,
                ContentHash = entity.Properties.ContentHash,
                ContentLanguage = entity.Properties.ContentLanguage,
                ContentType = entity.Properties.ContentType
            };
            await Context.GetBlobClient(cloudBlob.Name).SetHttpHeadersAsync(blobHttpHeaders).NoContext();
            await Context.GetBlobClient(cloudBlob.Name).SetMetadataAsync(entity.Properties.Metadata).NoContext();
            return true;
        }
        private protected async Task<IList<TEntity>> ReadAsync(BlobItem blobItem)
            => await this.ReadAsync(this.Context.GetBlobClient(blobItem.Name)).NoContext();
        private protected async Task<IList<TEntity>> ReadAsync(BlobBaseClient client)
        {
            Response<BlobDownloadInfo> item = await client.DownloadAsync();
            DataWrapper wrapper = new DataWrapper()
            {
                Name = client.Name,
                Properties = item.Value.Details.ToDataProperties(item.Value.ContentType),
                Stream = item.Value.Content
            };
            return (await this.Reader.ReadAsync(wrapper).NoContext()).Entities;
        }
    }
    public static class BlobExtesions
    {
        public static DataProperties ToDataProperties(this BlobItemProperties blobHttpHeaders, IDictionary<string, string> metadata)
        {
            return new DataProperties()
            {
                CacheControl = blobHttpHeaders.CacheControl,
                ContentDisposition = blobHttpHeaders.ContentDisposition,
                ContentEncoding = blobHttpHeaders.ContentEncoding,
                ContentHash = blobHttpHeaders.ContentHash,
                ContentLanguage = blobHttpHeaders.ContentLanguage,
                ContentType = blobHttpHeaders.ContentType,
                Metadata = metadata
            };
        }
        public static DataProperties ToDataProperties(this BlobDownloadDetails blobDownloadDetails, string contentType)
        {
            return new DataProperties()
            {
                CacheControl = blobDownloadDetails.CacheControl,
                ContentDisposition = blobDownloadDetails.ContentDisposition,
                ContentEncoding = blobDownloadDetails.ContentEncoding,
                ContentHash = blobDownloadDetails.BlobContentHash,
                ContentLanguage = blobDownloadDetails.ContentLanguage,
                ContentType = contentType,
                Metadata = blobDownloadDetails.Metadata
            };
        }
    }
}