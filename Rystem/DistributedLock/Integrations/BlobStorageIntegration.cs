using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal class BlobStorageIntegration : ILockIntegration
    {
        private static readonly string LeaseGuidId = Guid.NewGuid().ToString();
        private readonly LockConfiguration Configuration;
        private static readonly object TrafficLight = new object();
        private static readonly MemoryStream EmptyStream = new MemoryStream(new byte[0]);
        private BlobClient context;
        private protected BlobClient Context
        {
            get
            {
                if (context != null)
                    return context;
                BlobContainerClient container;
                lock (TrafficLight)
                {
                    if (context != null)
                        return context;
                    var client = new BlobServiceClient(Configuration.ConnectionString);
                    container = client.GetBlobContainerClient(Configuration.Name?.ToLower() ?? "lock");
                    context = container.GetBlobClient($"Lock_{this.Name}");
                }
                if (!container.Exists())
                    container.CreateIfNotExists();
                if (!context.Exists())
                    context.Upload(EmptyStream);
                return context;
            }
        }
        private readonly string Name;
        public BlobStorageIntegration(LockConfiguration lockConfiguration, string name)
        {
            this.Configuration = lockConfiguration;
            this.Name = name;
        }
        private BlobLeaseClient TokenAcquired;
        private static readonly object AcquireTrafficLight = new object();
        public bool Acquire()
        {
            try
            {
                var lease = this.Context.GetBlobLeaseClient(LeaseGuidId);
                if (this.TokenAcquired == null)
                    lock (AcquireTrafficLight)
                    {
                        if (this.TokenAcquired == null)
                        {
                            Response<BlobLease> response = lease.Acquire(new TimeSpan(-1));
                            this.TokenAcquired = lease;
                            return true;
                        }
                    }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool IsAcquired()
        {
            if (this.TokenAcquired != null)
                return true;
            Response<BlobProperties> properties = this.Context.GetProperties();
            return properties.Value.LeaseStatus == LeaseStatus.Locked;
        }
        private static readonly object ReleaseTrafficLight = new object();
        public bool Release()
        {
            if (TokenAcquired != null)
            {
                lock (ReleaseTrafficLight)
                {
                    if (TokenAcquired != null)
                    {
                        _ = TokenAcquired.Release();
                        TokenAcquired = null;
                    }
                }
            }
            return true;
        }
    }
}