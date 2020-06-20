using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockIntegration
    {
        bool Acquire();
        bool IsAcquired();
        bool Release();
    }
}