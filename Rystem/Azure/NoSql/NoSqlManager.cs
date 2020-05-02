
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal class NoSqlManager<TEntity> : INoSqlManager
        where TEntity : INoSql
    {
        private readonly IDictionary<Installation, INoSqlIntegration<TEntity>> Integrations;
        private readonly IDictionary<Installation, NoSqlConfiguration> NoSqlConfiguration;
        public NoSqlManager()
        {
            Integrations = new Dictionary<Installation, INoSqlIntegration<TEntity>>();
            NoSqlConfiguration = NoSqlInstaller.GetConfiguration<TEntity>();
            foreach (KeyValuePair<Installation, NoSqlConfiguration> configuration in NoSqlConfiguration)
                switch (configuration.Value.Type)
                {
                    case NoSqlType.TableStorage:
                        Integrations.Add(configuration.Key, new TableStorageIntegration<TEntity>(configuration.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Wrong type installed {configuration.Value.Type}");
                }
        }
        public async Task<bool> DeleteAsync(INoSql entity, Installation installation)
            => await Integrations[installation].DeleteAsync((TEntity)entity).NoContext();
        public async Task<bool> DeleteBatchAsync(IEnumerable<INoSql> entity, Installation installation)
             => await Integrations[installation].DeleteBatchAsync(entity.Select(x => (TEntity)x)).NoContext();
        public async Task<bool> ExistsAsync(INoSql entity, Installation installation)
            => await Integrations[installation].ExistsAsync((TEntity)entity).NoContext();
        public async Task<IList<TNoSqlEntity>> GetAsync<TNoSqlEntity>(INoSql entity, Installation installation, Expression<Func<TNoSqlEntity, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSql
            => await Integrations[installation].GetAsync((TEntity)entity, expression, takeCount).NoContext();
        public async Task<bool> UpdateAsync(INoSql entity, Installation installation)
            => await Integrations[installation].UpdateAsync((TEntity)entity).NoContext();
        public async Task<bool> UpdateBatchAsync(IEnumerable<INoSql> entity, Installation installation)
            => await Integrations[installation].UpdateBatchAsync(entity.Select(x => (TEntity)x)).NoContext();
        public string GetName(Installation installation = Installation.Default)
            => NoSqlConfiguration[installation].Name;
    }
}
