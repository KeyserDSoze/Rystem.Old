using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.Data
{
    internal class DataManager<TEntity> : IDataManager<TEntity>
        where TEntity : IData
    {
        private readonly IDictionary<Installation, IDataIntegration<TEntity>> Integrations = new Dictionary<Installation, IDataIntegration<TEntity>>();
        private readonly IDictionary<Installation, DataConfiguration> AggregatedDataConfiguration;
        private static readonly object TrafficLight = new object();
        private IDataIntegration<TEntity> Integration(Installation installation)
        {
            if (!Integrations.ContainsKey(installation))
                lock (TrafficLight)
                    if (!Integrations.ContainsKey(installation))
                    {
                        DataConfiguration configuration = AggregatedDataConfiguration[installation];
                        switch (configuration.Type)
                        {
                            case DataType.BlockBlob:
                                Integrations.Add(installation, new BlockBlobStorageIntegration<TEntity>(configuration, this.DefaultEntity));
                                break;
                            case DataType.AppendBlob:
                                Integrations.Add(installation, new AppendBlobStorageIntegration<TEntity>(configuration, this.DefaultEntity));
                                break;
                            case DataType.PageBlob:
                                Integrations.Add(installation, new PageBlobStorageIntegration<TEntity>(configuration, this.DefaultEntity));
                                break;
                            case DataType.DataLakeV2:
                                throw new NotImplementedException("DataLake V2 not implemented.");
                            default:
                                throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
                        }
                    }
            return Integrations[installation];
        }
        public InstallerType InstallerType => InstallerType.Data;
        private readonly TEntity DefaultEntity;

        public DataManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            AggregatedDataConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as DataConfiguration);
            this.DefaultEntity = entity;
        }
        public async Task<bool> DeleteAsync(TEntity entity, Installation installation)
            => await Integration(installation).DeleteAsync(entity).NoContext();
        public async Task<bool> ExistsAsync(TEntity entity, Installation installation)
            => await Integration(installation).ExistsAsync(entity).NoContext();
        public async Task<TEntity> FetchAsync(TEntity entity, Installation installation)
            => await Integration(installation).FetchAsync(entity).NoContext();
        public async Task<IEnumerable<TEntity>> ListAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integration(installation).ListAsync(entity, prefix, takeCount).NoContext();
        public async Task<IList<string>> SearchAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integration(installation).SearchAsync(entity, prefix, takeCount).NoContext();
        public async Task<IList<DataWrapper>> FetchPropertiesAsync(TEntity entity, Installation installation, string prefix, int? takeCount)
            => await Integration(installation).FetchPropertiesAsync(entity, prefix, takeCount).NoContext();
        public async Task<bool> WriteAsync(TEntity entity, Installation installation, long offset)
            => await Integration(installation).WriteAsync(entity, offset).NoContext();
        public string GetName(Installation installation)
            => this.AggregatedDataConfiguration[installation].Name;
    }
}
