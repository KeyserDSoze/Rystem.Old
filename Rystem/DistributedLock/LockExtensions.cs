using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    public static class LockExtensions
    {
        private static IRystemManager<TEntity> GetLockManager<TEntity>(TEntity entity)
            where TEntity : ILock
            => new LockManager<TEntity>(entity.GetConfigurationBuilder(), entity);
        private static ILockManager<TEntity> Manager<TEntity>(this TEntity entity)
            where TEntity : ILock
            => entity.DefaultManager<TEntity>(GetLockManager) as ILockManager<TEntity>;

        public static async Task<bool> AcquireAsync<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
            where TEntity : ILock
            => await entity.Manager().AcquireAsync(key, installation).NoContext();
        public static async Task<bool> ReleaseAsync<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
            where TEntity : ILock
            => await entity.Manager().ReleaseAsync(key, installation).NoContext();
        public static async Task<bool> IsAcquiredAsync<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
          where TEntity : ILock
          => await entity.Manager().IsAcquiredAsync(key, installation).NoContext();

        public static bool Acquire<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
            where TEntity : ILock
            => entity.AcquireAsync(key, installation).ToResult();
        public static bool Release<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
            where TEntity : ILock
            => entity.ReleaseAsync(key, installation).ToResult();
        public static bool IsAcquired<TEntity>(this TEntity entity, string key = null, Installation installation = Installation.Default)
           where TEntity : ILock
           => entity.IsAcquiredAsync(key, installation).ToResult();
    }
}