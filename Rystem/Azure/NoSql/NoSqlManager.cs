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
        public NoSqlManager()
        {
            NoSqlConfiguration configuration = NoSqlInstaller.GetConfiguration<TEntity>();
            switch (configuration.Type)
            {
                case NoSqlType.TableStorage:
                    Integration = new TableStorageIntegration<TEntity>(configuration);
                    break;
                default:
                    throw new InvalidOperationException($"Wrong type installed {configuration.Type}");
            }
        }
        public async Task<bool> DeleteAsync(INoSql entity)
            => await Integration.DeleteAsync(entity);
        public async Task<bool> ExistsAsync(INoSql entity)
            => await Integration.ExistsAsync(entity);
        public async Task<IEnumerable<TNoSqlEntity>> FetchAsync<TNoSqlEntity>(INoSql entity, Expression<Func<INoSql, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSql
            => (await Integration.GetAsync(entity, expression, takeCount)).Select(x => (TNoSqlEntity)(INoSql)x);
        public async Task<bool> UpdateAsync(INoSql entity)
            => await Integration.UpdateAsync(entity);
    }
}
