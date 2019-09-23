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
            DataLakeConfiguration<TEntity> configuration = DataLakeInstaller.GetConfiguration<TEntity>();
            switch (configuration.Type)
            {
                case DataLakeType.BlockBlob:
                    Integration = new BlockBlobStorageIntegration<TEntity>(configuration);
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

        public async Task<IList<IDataLake>> ListAsync(IDataLake entity, string prefix = null, int? takeCount = null)
            => await Integration.ListAsync(entity, prefix, takeCount);

        public async Task<IList<string>> SearchAsync(IDataLake entity, string prefix = null, int? takeCount = null)
            => await Integration.SearchAsync(entity, prefix, takeCount);

        public async Task<bool> AppendAsync(IDataLake entity)
            => await Integration.AppendAsync(entity);
        public async Task<string> WriteAsync(IDataLake entity)
            => await Integration.WriteAsync(entity);
    }
}
