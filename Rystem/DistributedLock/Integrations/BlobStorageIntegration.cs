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
        private static readonly RaceCondition AcquiringRaceCondition = new RaceCondition();
        public async Task<bool> AcquireAsync()
        {
            try
            {
                var lease = this.Context.GetBlobLeaseClient(LeaseGuidId);
                if (this.TokenAcquired == null)
                {
                    RaceConditionResponse response = await AcquiringRaceCondition.ExecuteAsync(async () =>
                    {
                        Response<BlobLease> response = await lease.AcquireAsync(new TimeSpan(0, 1, 0));
                        this.TokenAcquired = lease;
                    });
                    return response.IsExecuted && !response.InException;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> IsAcquiredAsync()
        {
            if (this.TokenAcquired != null)
                return true;
            Response<BlobProperties> properties = await this.Context.GetPropertiesAsync();
            return properties.Value.LeaseStatus == LeaseStatus.Locked;
        }
        private static readonly RaceCondition ReleasingRaceCondition = new RaceCondition();
        public async Task<bool> ReleaseAsync()
        {
            if (TokenAcquired != null)
                await ReleasingRaceCondition.ExecuteAsync(async () =>
                {
                    _ = await TokenAcquired.ReleaseAsync();
                    TokenAcquired = null;
                });
            return true;
        }
    }
}