using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal class DataManager<TEntity> : IDataManager
        where TEntity : IData
    {
        private readonly IDictionary<Installation, IDataIntegration<TEntity>> Integrations;
        private readonly IDictionary<Installation, DataConfiguration<TEntity>> AggregatedDataConfiguration;
        public DataManager()
        {
            Integrations = new Dictionary<Installation, IDataIntegration<TEntity>>();
            AggregatedDataConfiguration = DataInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, DataConfiguration<TEntity>> configuration in AggregatedDataConfiguration)
                switch (configuration.Value.Type)
                {
                    case AggregatedDataType.BlockBlob:
                        Integrations.Add(configuration.Key, new BlockBlobStorageIntegration<TEntity>(configuration.Value));
                        break;
                    case AggregatedDataType.AppendBlob:
                        Integrations.Add(configuration.Key, new AppendBlobStorageIntegration<TEntity>(configuration.Value));
                        break;
                    case AggregatedDataType.PageBlob:
                        Integrations.Add(configuration.Key, new PageBlobStorageIntegration<TEntity>(configuration.Value));
                        break;
                    case AggregatedDataType.DataLakeV2:
                        throw new NotImplementedException("DataLake V2 not implemented.");
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> DeleteAsync(IData entity, Installation installation)
            => await Integrations[installation].DeleteAsync(entity).NoContext();
        public async Task<bool> ExistsAsync(IData entity, Installation installation)
            => await Integrations[installation].ExistsAsync(entity).NoContext();
        public async Task<TEntityLake> FetchAsync<TEntityLake>(IData entity, Installation installation)
            where TEntityLake : IData
            => (TEntityLake)(IData)(await Integrations[installation].FetchAsync(entity).NoContext());
        public async Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IData entity, Installation installation, string prefix, int? takeCount)
            where TEntityLake : IData
            => (await Integrations[installation].ListAsync(entity, prefix, takeCount).NoContext()).Select(x => (TEntityLake)(IData)x);
        public async Task<IList<string>> SearchAsync(IData entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].SearchAsync(entity, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(IData entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].FetchPropertiesAsync(entity, prefix, takeCount).NoContext();
        public async Task<bool> WriteAsync(IData entity, Installation installation, long offset)
            => await Integrations[installation].WriteAsync(entity, offset).NoContext();
        public string GetName(Installation installation)
            => this.AggregatedDataConfiguration[installation].Name;
    }
}
