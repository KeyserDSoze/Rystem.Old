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
    internal static class BlobStorageBaseIntegration
    {
        internal static BlobRequestOptions BlobRequestOptions = new BlobRequestOptions() { DisableContentMD5Validation = true };
        public static async Task<bool> DeleteAsync(ICloudBlob cloudBlob)
            => await cloudBlob.DeleteIfExistsAsync().NoContext();

        public static async Task<bool> ExistsAsync(ICloudBlob cloudBlob)
            => await cloudBlob.ExistsAsync().NoContext();

        public static async Task<IList<string>> SearchAsync(CloudBlobContainer context, string prefix = null, int? takeCount = null)
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
        public static async Task<IList<DataWrapper>> FetchPropertiesAsync(CloudBlobContainer context, string prefix = null, int? takeCount = null)
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
                    items.Add(new DataWrapper() { Name = blobItem.Uri.AbsolutePath, Properties = (blobItem as ICloudBlob).Properties.ToAggregatedDataProperties() });
                }
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        public static async Task<bool> SetBlobPropertyIfNecessaryAsync(IData entity, ICloudBlob cloudBlob, DataWrapper wrapper)
        {
            BlobDataProperties blobDataProperties = wrapper.Properties as BlobDataProperties;
            bool changeSomethingInProperty = false;
            if (blobDataProperties != null)
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
        public static async Task<Stream> ReadAsync(ICloudBlob cloudBlob)
        {
            return await cloudBlob.OpenReadAsync(null, BlobRequestOptions, null).NoContext();
        }
        internal static BlobDataProperties ToAggregatedDataProperties(this BlobProperties blobProperties)
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
