using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData.Integration
{
    internal static class BlobStorageBaseIntegration
    {
        internal static BlobRequestOptions BlobRequestOptions = new BlobRequestOptions() { DisableContentMD5Validation = true };
        public static async Task<bool> DeleteAsync(ICloudBlob cloudBlob)
            => await cloudBlob.DeleteIfExistsAsync();

        public static async Task<bool> ExistsAsync(ICloudBlob cloudBlob)
            => await cloudBlob.ExistsAsync();

        public static async Task<IList<string>> SearchAsync(CloudBlobContainer context, string prefix = null, int? takeCount = null)
        {
            IList<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (var blobItem in segment.Results)
                    items.Add(blobItem.StorageUri.PrimaryUri.ToString());
                if (takeCount != null && items.Count >= takeCount)
                    break;
            } while (token != null);
            return items;
        }

        public static async Task<bool> SetBlobPropertyIfNecessary(IAggregatedData entity, ICloudBlob cloudBlob, AggregatedDataDummy dummy)
        {
            bool changeSomethingInProperty = false;
            if (dummy.Properties != null)
            {
                if (dummy.Properties.ContentType != cloudBlob.Properties.ContentType)
                {
                    cloudBlob.Properties.ContentType = dummy.Properties.ContentType ?? MimeMapping.GetMimeMapping(entity.Name);
                    changeSomethingInProperty = true;
                }
                if (dummy.Properties.CacheControl != null && dummy.Properties.CacheControl != cloudBlob.Properties.CacheControl)
                {
                    cloudBlob.Properties.CacheControl = dummy.Properties.CacheControl;
                    changeSomethingInProperty = true;
                }
                if (dummy.Properties.ContentDisposition != null && dummy.Properties.ContentDisposition != cloudBlob.Properties.ContentDisposition)
                {
                    cloudBlob.Properties.ContentDisposition = dummy.Properties.ContentDisposition;
                    changeSomethingInProperty = true;
                }
                if (dummy.Properties.ContentEncoding != null && dummy.Properties.ContentEncoding != cloudBlob.Properties.ContentEncoding)
                {
                    cloudBlob.Properties.ContentEncoding = dummy.Properties.ContentEncoding;
                    changeSomethingInProperty = true;
                }
                if (dummy.Properties.ContentLanguage != null && dummy.Properties.ContentLanguage != cloudBlob.Properties.ContentLanguage)
                {
                    cloudBlob.Properties.ContentLanguage = dummy.Properties.ContentLanguage;
                    changeSomethingInProperty = true;
                }
                if (dummy.Properties.ContentMD5 != null && dummy.Properties.ContentMD5 != cloudBlob.Properties.ContentMD5)
                {
                    cloudBlob.Properties.ContentMD5 = dummy.Properties.ContentMD5;
                    changeSomethingInProperty = true;
                }
            }
            if (changeSomethingInProperty)
                await cloudBlob.SetPropertiesAsync();
            return changeSomethingInProperty;
        }
        public static async Task<Stream> ReadAsync(ICloudBlob cloudBlob)
        {
            await cloudBlob.FetchAttributesAsync();
            var fileLength = cloudBlob.Properties.Length;
            byte[] fileByte = new byte[fileLength];
            await cloudBlob.DownloadToByteArrayAsync(fileByte, 0, null, BlobRequestOptions, null);
            Stream stream = new MemoryStream(fileByte);
            return stream;
        }
        internal static AggregatedDataProperties ToAggregatedDataProperties(this BlobProperties blobProperties)
        {
            return new AggregatedDataProperties(blobProperties.BlobTierLastModifiedTime,
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
