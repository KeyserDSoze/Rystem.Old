﻿using Azure;
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
        private static readonly object BlobTrafficLight = new object();
        private static readonly object ContainerTrafficLight = new object();
        private static readonly MemoryStream EmptyStream = new MemoryStream(new byte[0]);
        private BlobContainerClient container;
        private BlobContainerClient Container
        {
            get
            {
                if (container != null)
                    return container;
                lock (ContainerTrafficLight)
                {
                    if (container != null)
                        return container;
                    var client = new BlobServiceClient(Configuration.ConnectionString);
                    container = client.GetBlobContainerClient(Configuration.Name?.ToLower() ?? "lock");
                }
                return container;
            }
        }

        private BlobClient blob;
        private protected BlobClient Blob
        {
            get
            {
                if (blob != null)
                    return blob;
                lock (BlobTrafficLight)
                {
                    if (blob != null)
                        return blob;
                    blob = this.Container.GetBlobClient($"Lock_{Configuration.Key ?? this.Name}");
                }
                if (!this.Container.Exists())
                    this.Container.CreateIfNotExists();
                if (!blob.Exists())
                    blob.Upload(EmptyStream);
                return blob;
            }
        }
        private bool ContainerExistsCheck = false;
        private async Task<BlobClient> GetClientAsync(string key)
        {
            BlobClient keyBlob = this.Container.GetBlobClient($"Lock_{key}");
            if (!this.ContainerExistsCheck)
            {
                if (!await this.Container.ExistsAsync().NoContext())
                    await this.Container.CreateIfNotExistsAsync().NoContext();
                this.ContainerExistsCheck = true;
            }
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
        private static readonly RaceCondition AcquiringRaceCondition = new RaceCondition();
        public async Task<bool> AcquireAsync(string key)
        {
            try
            {
                var blob = key == null ? this.Blob : await this.GetClientAsync(key).NoContext();
                var lease = blob.GetBlobLeaseClient(LeaseGuidId);
                string normalizedKey = key ?? string.Empty;
                if (!this.TokenAcquireds.ContainsKey(normalizedKey))
                {
                    RaceConditionResponse response = await AcquiringRaceCondition.ExecuteAsync(async () =>
                    {
                        Response<BlobLease> response = await lease.AcquireAsync(new TimeSpan(0, 1, 0));
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
            var blob = key == null ? this.Blob : await this.GetClientAsync(key).NoContext();
            Response<BlobProperties> properties = await blob.GetPropertiesAsync().NoContext();
            return properties.Value.LeaseStatus == LeaseStatus.Locked;
        }
        private static readonly RaceCondition ReleasingRaceCondition = new RaceCondition();
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