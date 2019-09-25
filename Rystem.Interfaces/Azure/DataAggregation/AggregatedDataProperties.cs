using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public sealed class AggregatedDataProperties
    {
        public AggregatedDataProperties() { }
        public AggregatedDataProperties(DateTimeOffset? blobTierLastModifiedTime,
             bool? blobTierInferred,
             bool isIncrementalCopy,
             bool isServerEncrypted,
             int? appendBlobCommittedBlockCount,
              long? pageBlobSequenceNumber,
              int leaseDuration,
              int leaseState,
              int leaseStatus,
              DateTimeOffset? lastModified,
              DateTimeOffset? created,
              string eTag,
              DateTimeOffset? deletedTime,
              long length,
            int? remainingDaysBeforePermanentDelete)
        {
            this.BlobTierLastModifiedTime = blobTierLastModifiedTime;
            this.BlobTierInferred = blobTierInferred;
            this.IsIncrementalCopy = isIncrementalCopy;
            this.IsServerEncrypted = isServerEncrypted;
            this.AppendBlobCommittedBlockCount = appendBlobCommittedBlockCount;
            this.PageBlobSequenceNumber = pageBlobSequenceNumber;
            this.LeaseDuration = leaseDuration;
            this.LeaseState = leaseState;
            this.LeaseStatus = leaseStatus;
            this.LastModified = lastModified;
            this.Created = created;
            this.ETag = eTag;
            this.DeletedTime = deletedTime;
            this.Length = length;
            this.RemainingDaysBeforePermanentDelete = remainingDaysBeforePermanentDelete;
        }
        public DateTimeOffset? BlobTierLastModifiedTime { get; }
        public bool? BlobTierInferred { get; }
        public bool IsIncrementalCopy { get; }
        public bool IsServerEncrypted { get; }
        public int? AppendBlobCommittedBlockCount { get; }
        public long? PageBlobSequenceNumber { get; }
        public int LeaseDuration { get; }
        public int LeaseState { get; }
        public int LeaseStatus { get; }
        public DateTimeOffset? LastModified { get; }
        public DateTimeOffset? Created { get; }
        public string ETag { get; }
        public DateTimeOffset? DeletedTime { get; }
        public long Length { get; }
        public int? RemainingDaysBeforePermanentDelete { get; }
        public string ContentType { get; set; }
        public string ContentMD5 { get; set; }
        public string ContentLanguage { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentDisposition { get; set; }
        public string CacheControl { get; set; }
    }
}
