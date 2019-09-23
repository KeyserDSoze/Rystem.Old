using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal class NoSqlManager<TEntity> : INoSqlManager
        where TEntity : INoSqlStorage
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
        public async Task<bool> DeleteAsync(INoSqlStorage entity)
            => await Integration.DeleteAsync(entity);
        public async Task<bool> ExistsAsync(INoSqlStorage entity)
            => await Integration.ExistsAsync(entity);
        public async Task<IEnumerable<TNoSqlEntity>> FetchAsync<TNoSqlEntity>(INoSqlStorage entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            where TNoSqlEntity : INoSqlStorage
            => (await Integration.GetAsync(entity, expression, takeCount)).Select(x => (TNoSqlEntity)(INoSqlStorage)x);
        public async Task<bool> UpdateAsync(INoSqlStorage entity)
            => await Integration.UpdateAsync(entity);
    }
}
