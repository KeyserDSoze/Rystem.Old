using System;
using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockManager<TEntity> : IManager<TEntity>
    {
        bool Acquire(Installation installation = Installation.Default);
        bool IsAcquired(Installation installation = Installation.Default);
        bool Release(Installation installation = Installation.Default);
    }
}