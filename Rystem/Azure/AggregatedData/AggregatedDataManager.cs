using Rystem.Enums;
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
        private readonly IDictionary<Installation, IAggregatedDataIntegration<TEntity>> Integrations;
        private readonly IDictionary<Installation, AggregatedDataConfiguration<TEntity>> AggregatedDataConfiguration;
        public AggregatedDataManager()
        {
            Integrations = new Dictionary<Installation, IAggregatedDataIntegration<TEntity>>();
            AggregatedDataConfiguration = AggregatedDataInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, AggregatedDataConfiguration<TEntity>> configuration in AggregatedDataConfiguration)
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
        public async Task<bool> DeleteAsync(IAggregatedData entity, Installation installation)
            => await Integrations[installation].DeleteAsync(entity);
        public async Task<bool> ExistsAsync(IAggregatedData entity, Installation installation)
            => await Integrations[installation].ExistsAsync(entity);
        public async Task<TEntityLake> FetchAsync<TEntityLake>(IAggregatedData entity, Installation installation)
            where TEntityLake : IAggregatedData
            => (TEntityLake)(IAggregatedData)(await Integrations[installation].FetchAsync(entity));
        public async Task<IEnumerable<TEntityLake>> ListAsync<TEntityLake>(IAggregatedData entity, Installation installation, string prefix, int? takeCount)
            where TEntityLake : IAggregatedData
            => (await Integrations[installation].ListAsync(entity, prefix, takeCount)).Select(x => (TEntityLake)(IAggregatedData)x);
        public async Task<IList<string>> SearchAsync(IAggregatedData entity, Installation installation, string prefix, int? takeCount)
            => await Integrations[installation].SearchAsync(entity, prefix, takeCount);
        public async Task<bool> WriteAsync(IAggregatedData entity, Installation installation, long offset)
            => await Integrations[installation].WriteAsync(entity, offset);
        public string GetName(Installation installation)
            => this.AggregatedDataConfiguration[installation].Name;
    }
}
