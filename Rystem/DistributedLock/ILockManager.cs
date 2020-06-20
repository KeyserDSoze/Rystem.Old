using System;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockManager<TEntity> : IManager<TEntity>
    {
        Task<bool> AcquireAsync(Installation installation = Installation.Default);
        Task<bool> IsAcquiredAsync(Installation installation = Installation.Default);
        Task<bool> ReleaseAsync(Installation installation = Installation.Default);
    }
}