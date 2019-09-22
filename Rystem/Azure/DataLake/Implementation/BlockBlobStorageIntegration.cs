using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.DataLake
{
    internal class BlockBlobStorageIntegration : IDataLakeIntegration
    {
        private CloudBlobContainer Context;
        private IDataLakeReader Reader;
        private IDataLakeWriter Writer;
        internal BlockBlobStorageIntegration(DataLakeConfiguration configuration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            this.Context = Client.GetContainerReference(configuration.Name.ToLower());
            this.Context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.Reader = configuration.Reader;
            this.Writer = configuration.Writer;
        }
        public Task<bool> DeleteAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<IDataLake> FetchAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<IList<IDataLake>> ListAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> SearchAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }
    }
}
