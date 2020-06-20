using System.Threading.Tasks;

namespace System
{
    public static class Retry
    {
        public static Retryable<object> Create(Func<Task> action, int maxAttempts = 3)
            => new Retryable<object>(async () => { await action.Invoke(); return new object(); }, maxAttempts);
        public static Retryable<T> Create<T>(Func<Task<T>> action, int maxAttempts = 3)
            => new Retryable<T>(action, maxAttempts);
    }
    public static class RetryExtensions
    {
        public static Retryable<T> Retry<T>(this Func<Task<T>> action, int maxAttempts = 3)
            => new Retryable<T>(action, maxAttempts);
        public static Retryable<object> Retry(this Func<Task> action, int maxAttempts = 3)
            => new Retryable<object>(async () => { await action.Invoke(); return new object(); }, maxAttempts);
    }
}