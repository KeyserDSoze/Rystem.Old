using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal class NoSqlManager<TEntity> : INoSqlManager<TEntity>, IManager<TEntity>
    {
        private readonly IDictionary<Installation, INoSqlIntegration<TEntity>> Integrations;
        private readonly IDictionary<Installation, NoSqlConfiguration> NoSqlConfiguration;

        public InstallerType InstallerType => InstallerType.NoSql;

        public NoSqlManager(ConfigurationBuilder configurationBuilder, TEntity entity)
        {
            Integrations = new Dictionary<Installation, INoSqlIntegration<TEntity>>();
            NoSqlConfiguration = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as NoSqlConfiguration);
            foreach (KeyValuePair<Installation, NoSqlConfiguration> configuration in NoSqlConfiguration)
                switch (configuration.Value.Type)
                {
                    case NoSqlType.TableStorage:
                        Integrations.Add(configuration.Key, new TableStorageIntegration<TEntity>(configuration.Value, entity));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> DeleteAsync(TEntity entity, Installation installation)
            => await Integrations[installation].DeleteAsync(entity).NoContext();
        public async Task<bool> DeleteBatchAsync(IEnumerable<TEntity> entity, Installation installation)
             => await Integrations[installation].DeleteBatchAsync(entity).NoContext();
        public async Task<bool> ExistsAsync(TEntity entity, Installation installation)
            => await Integrations[installation].ExistsAsync(entity).NoContext();
        public async Task<IList<TEntity>> GetAsync(TEntity entity, Installation installation, Expression<Func<TEntity, bool>> expression = null, int? takeCount = null)
            => await Integrations[installation].GetAsync(entity, expression, takeCount).NoContext();
        public async Task<bool> UpdateAsync(TEntity entity, Installation installation)
            => await Integrations[installation].UpdateAsync(entity).NoContext();
        public async Task<bool> UpdateBatchAsync(IEnumerable<TEntity> entity, Installation installation)
            => await Integrations[installation].UpdateBatchAsync(entity).NoContext();
        public string GetName(Installation installation = Installation.Default)
            => NoSqlConfiguration[installation].Name;
    }
}
