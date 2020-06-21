using System.Threading.Tasks;

namespace Rystem.DistributedLock
{
    internal interface ILockIntegration
    {
        Task<bool> AcquireAsync(string key);
        Task<bool> IsAcquiredAsync(string key);
        Task<bool> ReleaseAsync(string key);
    }
}