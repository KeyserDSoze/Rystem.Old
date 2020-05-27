using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    internal class DataManager<TEntity> : IDataManager<TEntity>
        where TEntity : IData
    {
        private readonly IDictionary<Installation, IDataIntegration<TEntity>> Integrations;
        private readonly IDictionary<Installation, DataConfiguration> AggregatedDataConfiguration;

        public InstallerType InstallerType => InstallerType.Data;

        public DataManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            Integrations = new Dictionary<Installation, IDataIntegration<TEntity>>();
            AggregatedDataConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as DataConfiguration);
            foreach (KeyValuePair<Installation, DataConfiguration> configuration in AggregatedDataConfiguration)
                switch (configuration.Value.Type)
                {
                    case DataType.BlockBlob:
                        Integrations.Add(configuration.Key, new BlockBlobStorageIntegration<TEntity>(configuration.Value, entity));
                        break;
                    case DataType.AppendBlob:
                        Integrations.Add(configuration.Key, new AppendBlobStorageIntegration<TEntity>(configuration.Value, entity));
                        break;
                    case DataType.PageBlob:
                        Integrations.Add(configuration.Key, new PageBlobStorageIntegration<TEntity>(configuration.Value, entity));
                        break;
                    case DataType.DataLakeV2:
                        throw new NotImplementedException("DataLake V2 not implemented.");
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> DeleteAsync(TEntity entity, Installation installation)
            => await Integrations[installation].DeleteAsync(entity).NoContext();
        public async Task<bool> ExistsAsync(TEntity entity, Installation installation)
            => await Integrations[installation].ExistsAsync(entity).NoContext();
        public async Task<TEntity> FetchAsync(TEntity entity, Installation installation)
            => await Integrations[installation].FetchAsync(entity).NoContext();
        public async Task<IEnumerable<TEntity>> ListAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].ListAsync(entity, prefix, takeCount).NoContext();
        public async Task<IList<string>> SearchAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].SearchAsync(entity, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].FetchPropertiesAsync(entity, prefix, takeCount).NoContext();
        public async Task<bool> WriteAsync(TEntity entity, Installation installation, long offset)
            => await Integrations[installation].WriteAsync(entity, offset).NoContext();
        public string GetName(Installation installation)
            => this.AggregatedDataConfiguration[installation].Name;
    }
}
