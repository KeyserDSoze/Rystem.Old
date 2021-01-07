using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.NoSql
{
    internal class BlobStorageIntegration<TEntity> : INoSqlIntegration<TEntity>
    {
        private protected BlobContainerClient context;
        private readonly RaceCondition RaceCondition = new RaceCondition();
        private protected async Task<BlobContainerClient> GetContextAsync()
        {
            if (context == null)
                await RaceCondition.ExecuteAsync(async () =>
                {
                    if (context == null)
                    {
                        var client = new BlobServiceClient(NoSqlConfiguration.ConnectionString);
                        var preContext = client.GetBlobContainerClient(NoSqlConfiguration.Name.ToLower());
                        if (!await preContext.ExistsAsync().NoContext())
                            await preContext.CreateIfNotExistsAsync().NoContext();
                        context = preContext;
                    }
                }).NoContext();
            return context;
        }
        private readonly IDictionary<string, PropertyInfo> KeyProperties = new Dictionary<string, PropertyInfo>();
        private const string Key = "Keys";
        private readonly NoSqlConfiguration NoSqlConfiguration;
        private readonly Type EntityType;
        internal BlobStorageIntegration(NoSqlConfiguration noSqlConfiguration, TEntity entity)
        {
            this.EntityType = entity.GetType();
            this.NoSqlConfiguration = noSqlConfiguration;
            if (this.NoSqlConfiguration.Name == null)
                this.NoSqlConfiguration.Name = this.EntityType.Name;
            foreach (PropertyInfo pi in this.EntityType.GetProperties())
            {
                if (pi.GetCustomAttribute(typeof(NoSqlProperty)) != null)
                    continue;
                if (pi.Name == Key)
                    KeyProperties.Add(pi.Name, pi);
            }
        }
        private string GetKey(TEntity entity)
        {
            var keys = (IEnumerable<string>)this.KeyProperties[Key].GetValue(entity);
            return keys == null ? string.Empty : string.Join("/", keys);
        }
        public async Task<bool> DeleteAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync().NoContext();
            return await client.GetBlobClient(this.GetKey(entity)).DeleteIfExistsAsync().NoContext();
        }

        public async Task<bool> ExistsAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync().NoContext();
            return await client.GetBlobClient(this.GetKey(entity)).ExistsAsync().NoContext();
        }

        public async Task<IList<TEntity>> GetAsync(TEntity entity, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null)
        {
            if (expression != null)
                throw new NotImplementedException("Rystem error. It's not possible to query inside Blob storage. Use a right path and the keys to understand better what you want.");
            var client = context ?? await GetContextAsync().NoContext();
            IList<TEntity> items = new List<TEntity>();
            CancellationToken token = default;
            int count = 0;
            await foreach (var t in client.GetBlobsAsync(BlobTraits.All, BlobStates.All, this.GetKey(entity), token))
            {
                if (t.Properties.BlobType == BlobType.Block)
                {
                    items.Add(await this.ReadAsync(t));
                    count++;
                }
                if (takeCount != null && items.Count >= takeCount)
                    break;
            }
            return items;
        }
        private protected async Task<TEntity> ReadAsync(BlobItem blobItem)
        {
            var client = context ?? await GetContextAsync().NoContext();
            var cloudBlob = await client.GetBlobClient(blobItem.Name).DownloadAsync().NoContext();
            using (StreamReader reader = new StreamReader(cloudBlob.Value.Content))
                return (await reader.ReadToEndAsync().NoContext()).FromDefaultJson<TEntity>();
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            var client = context ?? await GetContextAsync().NoContext();
            BlockBlobClient cloudBlob = client.GetBlockBlobClient(this.GetKey(entity));
            await cloudBlob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(entity.ToDefaultJson()))).NoContext();
            return true;
        }
        public Task<bool> UpdateBatchAsync(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException("Rystem error. It's not possible to update batch inside Blob storage.");
        }
        public Task<bool> DeleteBatchAsync(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException("Rystem error. It's not possible to delete batch inside Blob storage.");
        }
    }
}