using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal class BlobStorageIntegration : ILockIntegration
    {
        private static readonly string LeaseGuidId = Guid.NewGuid().ToString();
        private readonly LockConfiguration Configuration;
        private readonly RaceCondition BlobTrafficLight = new RaceCondition();
        private readonly RaceCondition ContainerTrafficLight = new RaceCondition();
        private static readonly MemoryStream EmptyStream = new MemoryStream(new byte[0]);
        private BlobContainerClient container;
        private async Task<BlobContainerClient> GetContainerAsync()
        {
            if (container == null)
                await ContainerTrafficLight.ExecuteAsync(async () =>
                {
                    if (container == null)
                    {
                        var client = new BlobServiceClient(Configuration.ConnectionString);
                        var preContainer = client.GetBlobContainerClient(Configuration.Name?.ToLower() ?? "lock");
                        if (!await preContainer.ExistsAsync().NoContext())
                            await preContainer.CreateIfNotExistsAsync().NoContext();
                        container = preContainer;
                    }
                }).NoContext();
            return container;
        }

        private BlobClient defaultBlob;
        private protected async Task<BlobClient> GetDefaultBlobClientAsync()
        {
            if (defaultBlob == null)
                await BlobTrafficLight.ExecuteAsync(async () =>
                {
                    if (defaultBlob == null)
                    {
                        var client = container ?? await this.GetContainerAsync().NoContext();
                        var preBlob = client.GetBlobClient($"Lock_{Configuration.Key ?? this.Name}");
                        if (!await preBlob.ExistsAsync().NoContext())
                            await preBlob.UploadAsync(EmptyStream).NoContext();
                        defaultBlob = preBlob;
                    }
                }).NoContext();
            return defaultBlob;
        }
        private async Task<BlobClient> GetClientAsync(string key)
        {
            BlobClient keyBlob = (container ?? await this.GetContainerAsync()).GetBlobClient($"Lock_{key}");
            if (!await keyBlob.ExistsAsync().NoContext())
                await keyBlob.UploadAsync(EmptyStream).NoContext();
            return keyBlob;
        }
        private readonly string Name;
        public BlobStorageIntegration(LockConfiguration lockConfiguration, string name)
        {
            this.Configuration = lockConfiguration;
            this.Name = name;
        }
        private ConcurrentDictionary<string, BlobLeaseClient> TokenAcquireds = new ConcurrentDictionary<string, BlobLeaseClient>();
        private readonly RaceCondition AcquiringRaceCondition = new RaceCondition();
        public async Task<bool> AcquireAsync(string key)
        {
            try
            {
                var blob = key == null ? (defaultBlob ?? await this.GetDefaultBlobClientAsync().NoContext()) : await this.GetClientAsync(key).NoContext();
                var lease = blob.GetBlobLeaseClient(LeaseGuidId);
                string normalizedKey = key ?? string.Empty;
                if (!this.TokenAcquireds.ContainsKey(normalizedKey))
                {
                    RaceConditionResponse response = await AcquiringRaceCondition.ExecuteAsync(async () =>
                    {
                        Response<BlobLease> response = await lease.AcquireAsync(new TimeSpan(0, 1, 0)).NoContext();
                        this.TokenAcquireds.TryAdd(normalizedKey, lease);
                    }).NoContext();
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
        public async Task<bool> IsAcquiredAsync(string key)
        {
            string normalizedKey = key ?? string.Empty;
            if (this.TokenAcquireds.ContainsKey(normalizedKey))
                return true;
            var blob = key == null ? (defaultBlob ?? await this.GetDefaultBlobClientAsync().NoContext()) : await this.GetClientAsync(key).NoContext();
            Response<BlobProperties> properties = await blob.GetPropertiesAsync().NoContext();
            return properties.Value.LeaseStatus == LeaseStatus.Locked;
        }
        private readonly RaceCondition ReleasingRaceCondition = new RaceCondition();
        public async Task<bool> ReleaseAsync(string key)
        {
            string normalizedKey = key ?? string.Empty;
            if (TokenAcquireds.ContainsKey(normalizedKey))
                await ReleasingRaceCondition.ExecuteAsync(async () =>
                {
                    _ = await TokenAcquireds[normalizedKey].ReleaseAsync().NoContext();
                    TokenAcquireds.TryRemove(normalizedKey, out _);
                }).NoContext();
            return true;
        }
    }
}