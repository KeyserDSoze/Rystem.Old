using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData
{
    internal class AggregatedDataManager<TEntity> : IAggregatedDataManager
        where TEntity : IAggregatedData
    {
        private readonly IAggregatedDataIntegration<TEntity> Integration;
        public AggregatedDataManager()
        {
            AggregatedDataConfiguration<TEntity> configuration = AggregatedDataInstaller.GetConfiguration<TEntity>();
            switch (configuration.Type)
            {
                case AggregatedDataType.BlockBlob:
                    Integration = new BlockBlobStorageIntegration<TEntity>(configuration);
                    break;
                case AggregatedDataType.AppendBlob:
                    Integration = new AppendBlobStorageIntegration<TEntity>(configuration);
                    break;
                case AggregatedDataType.PageBlob:
                    Integration = new PageBlobStorageIntegration<TEntity>(configuration);
                    break;
                case AggregatedDataType.DataLakeV2:
                    throw new NotImplementedException("DataLake V2 not implemented.");
                default:
                    throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
            }
        }
        public async Task<bool> DeleteAsync(IAggregatedData entity)
            => await Integration.DeleteAsync(entity);
        public async Task<bool> ExistsAsync(IAggregatedData entity)
            => await Integration.ExistsAsync(entity);
        public async Task<TEntityLake> FetchAsync<TEntityLake>(IAggregatedData entity)
            where TEntityLake : IAggregatedData
            => (TEntityLake)(IAggregatedData)(await Integration.FetchAsync(entity));
        public async Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IAggregatedData entity, string prefix = null, int? takeCount = null)
            where TEntityLake : IAggregatedData
            => (await Integration.ListAsync(entity, prefix, takeCount)).Select(x => (TEntityLake)(IAggregatedData)x);
        public async Task<IList<string>> SearchAsync(IAggregatedData entity, string prefix = null, int? takeCount = null)
            => await Integration.SearchAsync(entity, prefix, takeCount);
        public async Task<bool> AppendAsync(IAggregatedData entity, long offset = 0)
            => await Integration.AppendAsync(entity, offset);
        public async Task<string> WriteAsync(IAggregatedData entity)
            => await Integration.WriteAsync(entity);
    }
}
