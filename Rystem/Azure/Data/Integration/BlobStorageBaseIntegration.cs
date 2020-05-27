using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data.Integration
{
    internal abstract class BlobStorageBaseIntegration<TEntity>
        where TEntity : IData
    {
        private protected BlobRequestOptions BlobRequestOptions = new BlobRequestOptions() { DisableContentMD5Validation = true };
        private protected abstract IDataReader<TEntity> DefaultReader { get; }
        private protected abstract IDataWriter<TEntity> DefaultWriter { get; }
        private static readonly object TrafficLight = new object();
        private CloudBlobContainer context;
        private protected CloudBlobContainer Context
        {
            get
            {
                if (context != null)
                    return context;
                lock (TrafficLight)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration.ConnectionString);
                    CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
                    context = Client.GetContainerReference(Configuration.Name.ToLower());
                }
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
        private protected async Task<bool> DeleteAsync(ICloudBlob cloudBlob)
            => await cloudBlob.DeleteIfExistsAsync().NoContext();

        private protected async Task<bool> ExistsAsync(ICloudBlob cloudBlob)
            => await cloudBlob.ExistsAsync().NoContext();

        private protected async Task<IList<string>> SearchAsync(CloudBlobContainer context, string prefix = null, int? takeCount = null)
        {
            IList<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, takeCount, token, new BlobRequestOptions(), new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (var blobItem in segment.Results)
                    items.Add(blobItem.StorageUri.PrimaryUri.ToString());
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }
        private protected async Task<IList<DataWrapper>> FetchPropertiesAsync(CloudBlobContainer context, string prefix = null, int? takeCount = null)
        {
            IList<DataWrapper> items = new List<DataWrapper>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { }).NoContext();
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    await (blobItem as ICloudBlob).FetchAttributesAsync().NoContext();
                    items.Add(new DataWrapper() { Name = blobItem.Uri.AbsolutePath, Properties = GetDataProperties((blobItem as ICloudBlob).Properties) });
                }
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        private protected async Task<bool> SetBlobPropertyIfNecessaryAsync(IData entity, ICloudBlob cloudBlob, DataWrapper wrapper)
        {
            bool changeSomethingInProperty = false;
            if (wrapper.Properties is BlobDataProperties blobDataProperties)
            {
                if (blobDataProperties.ContentType != cloudBlob.Properties.ContentType)
                {
                    cloudBlob.Properties.ContentType = blobDataProperties.ContentType ?? MimeMapping.GetMimeMapping(entity.Name);
                    changeSomethingInProperty = true;
                }
                if (blobDataProperties.CacheControl != null && blobDataProperties.CacheControl != cloudBlob.Properties.CacheControl)
                {
                    cloudBlob.Properties.CacheControl = blobDataProperties.CacheControl;
                    changeSomethingInProperty = true;
                }
                if (blobDataProperties.ContentDisposition != null && blobDataProperties.ContentDisposition != cloudBlob.Properties.ContentDisposition)
                {
                    cloudBlob.Properties.ContentDisposition = blobDataProperties.ContentDisposition;
                    changeSomethingInProperty = true;
                }
                if (blobDataProperties.ContentEncoding != null && blobDataProperties.ContentEncoding != cloudBlob.Properties.ContentEncoding)
                {
                    cloudBlob.Properties.ContentEncoding = blobDataProperties.ContentEncoding;
                    changeSomethingInProperty = true;
                }
                if (blobDataProperties.ContentLanguage != null && blobDataProperties.ContentLanguage != cloudBlob.Properties.ContentLanguage)
                {
                    cloudBlob.Properties.ContentLanguage = blobDataProperties.ContentLanguage;
                    changeSomethingInProperty = true;
                }
                if (blobDataProperties.ContentMD5 != null && blobDataProperties.ContentMD5 != cloudBlob.Properties.ContentMD5)
                {
                    cloudBlob.Properties.ContentMD5 = blobDataProperties.ContentMD5;
                    changeSomethingInProperty = true;
                }
            }
            if (changeSomethingInProperty)
                await cloudBlob.SetPropertiesAsync().NoContext();
            return changeSomethingInProperty;
        }
        private protected async Task<IList<TEntity>> ReadAsync(ICloudBlob cloudBlob)
        {
            return (await this.Reader.ReadAsync(new DataWrapper()
            {
                Name = cloudBlob.Name,
                Stream = await cloudBlob.OpenReadAsync(null, BlobRequestOptions, null).NoContext(),
                Properties = this.GetDataProperties(cloudBlob.Properties)
            }).NoContext()).Entities;
        }
        private protected BlobDataProperties GetDataProperties(BlobProperties blobProperties)
        {
            return new BlobDataProperties(blobProperties.BlobTierLastModifiedTime,
                blobProperties.BlobTierInferred,
                blobProperties.IsIncrementalCopy,
                blobProperties.IsServerEncrypted,
                blobProperties.AppendBlobCommittedBlockCount,
                blobProperties.PageBlobSequenceNumber,
                (int)blobProperties.LeaseDuration,
                (int)blobProperties.LeaseState,
                (int)blobProperties.LeaseStatus,
                blobProperties.LastModified,
                blobProperties.Created,
                blobProperties.ETag,
                blobProperties.DeletedTime,
                blobProperties.Length,
                blobProperties.RemainingDaysBeforePermanentDelete
                )
            {
                CacheControl = blobProperties.CacheControl,
                ContentDisposition = blobProperties.ContentDisposition,
                ContentEncoding = blobProperties.ContentEncoding,
                ContentLanguage = blobProperties.ContentLanguage,
                ContentMD5 = blobProperties.ContentMD5,
                ContentType = blobProperties.ContentType,
            };
        }
    }
}
