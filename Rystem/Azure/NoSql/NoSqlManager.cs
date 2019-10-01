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
        private readonly INoSqlIntegration<TEntity> Integration;
        private readonly NoSqlConfiguration NoSqlConfiguration;
        public NoSqlManager()
        {
            NoSqlConfiguration = NoSqlInstaller.GetConfiguration<TEntity>();
            switch (NoSqlConfiguration.Type)
            {
                case NoSqlType.TableStorage:
                    Integration = new TableStorageIntegration<TEntity>(NoSqlConfiguration);
                    break;
                default:
                    throw new InvalidOperationException($"Wrong type installed {NoSqlConfiguration.Type}");
            }
        }
        public async Task<bool> DeleteAsync(INoSql entity)
            => await Integration.DeleteAsync((TEntity)entity);
        public async Task<bool> ExistsAsync(INoSql entity)
            => await Integration.ExistsAsync((TEntity)entity);
        public async Task<IEnumerable<TNoSqlEntity>> FetchAsync<TNoSqlEntity>(INoSql entity, Expression<Func<TNoSqlEntity, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSql
            => (await Integration.GetAsync((TEntity)entity, expression as Expression<Func<TEntity, bool>>, takeCount)).Select(x => (TNoSqlEntity)(INoSql)x);
        public async Task<bool> UpdateAsync(INoSql entity)
            => await Integration.UpdateAsync((TEntity)entity);
        public string GetName()
            => NoSqlConfiguration.Name;
    }
}
