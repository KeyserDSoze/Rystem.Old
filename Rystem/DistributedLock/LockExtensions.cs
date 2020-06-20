using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    public static class LockExtensions
    {
        private static IManager<TEntity> GetLockManager<TEntity>(TEntity entity)
            where TEntity : ILock
            => new LockManager<TEntity>(entity.GetConfigurationBuilder(), entity);
        private static ILockManager<TEntity> Manager<TEntity>(this TEntity entity)
            where TEntity : ILock
            => entity.DefaultManager<TEntity>(GetLockManager) as ILockManager<TEntity>;

        public static async Task<bool> AcquireAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ILock
            => await entity.Manager().AcquireAsync(installation).NoContext();
        public static async Task<bool> ReleaseAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ILock
            => await entity.Manager().ReleaseAsync(installation).NoContext();
        public static async Task<bool> IsAcquiredAsync<TEntity>(this TEntity entity, Installation installation = Installation.Default)
          where TEntity : ILock
          => await entity.Manager().IsAcquiredAsync(installation).NoContext();

        public static bool Acquire<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ILock
            => entity.AcquireAsync(installation).ToResult();
        public static bool Release<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ILock
            => entity.ReleaseAsync(installation).ToResult();
        public static bool IsAcquired<TEntity>(this TEntity entity, Installation installation = Installation.Default)
           where TEntity : ILock
           => entity.IsAcquiredAsync(installation).ToResult();
    }
}