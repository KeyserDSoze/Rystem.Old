using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockIntegration
    {
        Task<bool> AcquireAsync();
        Task<bool> ReleaseAsync();
    }
}