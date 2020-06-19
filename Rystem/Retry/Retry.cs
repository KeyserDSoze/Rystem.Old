using System;
using System.Threading.Tasks;

namespace Rystem
{
    public static class Retry
    {
        public static Retryable<T> Create<T>(Func<Task<T>> action, int maxAttempts = 3)
            => new Retryable<T>(action, maxAttempts);
    }
    
}