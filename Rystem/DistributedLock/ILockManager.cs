using System;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockManager<TEntity> : IRystemManager<TEntity>
    {
        Task<bool> AcquireAsync(string key, Installation installation = Installation.Default);
        Task<bool> IsAcquiredAsync(string key, Installation installation = Installation.Default);
        Task<bool> ReleaseAsync(string key, Installation installation = Installation.Default);
    }
}