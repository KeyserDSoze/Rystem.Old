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
                    context = container.GetBlobClient(this.Name);
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
        public async Task<bool> AcquireAsync()
        {
            try
            {
                var lease = this.Context.GetBlobLeaseClient();
                Response<BlobLease> response = await lease.AcquireAsync(new TimeSpan(15)).NoContext();
                this.TokenAcquired = lease;
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }
        public async Task<bool> ReleaseAsync()
        {
            if (TokenAcquired != null)
            {
                _ = await TokenAcquired.ReleaseAsync().NoContext();
                TokenAcquired = null;
            }
            return true;
        }
    }
}