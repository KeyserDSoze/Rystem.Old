using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.NoSql
{
    internal class NoSqlManager<TEntity> : INoSqlManager
        where TEntity : INoSqlStorage
    {
        private readonly static INoSqlIntegration Integration;
        static NoSqlManager()
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
        public async Task<IList<INoSqlStorage>> FetchAsync(INoSqlStorage entity, Expression<Func<INoSqlStorage, bool>> expression = null, int? takeCount = null)
            => await Integration.GetAsync(entity, expression, takeCount);
        public async Task<bool> UpdateAsync(INoSqlStorage entity)
            => await Integration.UpdateAsync(entity);
    }
}
