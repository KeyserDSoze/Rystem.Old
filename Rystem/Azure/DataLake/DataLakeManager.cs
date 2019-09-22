using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.DataLake
{
    internal class DataLakeManager<TEntity> : IDataLakeManager
        where TEntity : IDataLake
    {
        private readonly static IDataLakeIntegration Integration;
        static DataLakeManager()
        {
            DataLakeConfiguration configuration = DataLakeInstaller.GetConfiguration<TEntity>();
            switch (configuration.Type)
            {
                case DataLakeType.BlockBlob:
                    Integration = new BlockBlobStorageIntegration(configuration);
                    break;
                case DataLakeType.AppendBlob:
                    //Integration = new TableStorageIntegration<TEntity>(configuration);
                    break;
                case DataLakeType.PageBlob:
                    //Integration = new TableStorageIntegration<TEntity>(configuration);
                    break;
                case DataLakeType.DataLakeV2:
                    throw new NotImplementedException("DataLake V2 not implemented.");
                default:
                    throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
            }
        }
        public async Task<bool> DeleteAsync(IDataLake entity)
            => await Integration.DeleteAsync(entity);
        public async Task<bool> ExistsAsync(IDataLake entity)
            => await Integration.ExistsAsync(entity);
        public async Task<IDataLake> FetchAsync(IDataLake entity)
            => await Integration.FetchAsync(entity);

        public Task<IList<IDataLake>> ListAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> SearchAsync(IDataLake entity)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(IDataLake entity)
            => await Integration.UpdateAsync(entity);
    }
}
