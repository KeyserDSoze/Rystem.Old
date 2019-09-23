using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public sealed class LakeProperties
    {
        public LakeProperties() { }
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
        public string ContentType { get; set; }
        public string ContentMD5 { get; set; }
        public long Length { get; }
        public string ContentLanguage { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentDisposition { get; set; }
        public string CacheControl { get; set; }
        public DateTimeOffset? DeletedTime { get; }
        public int? RemainingDaysBeforePermanentDelete { get; }
    }
}
