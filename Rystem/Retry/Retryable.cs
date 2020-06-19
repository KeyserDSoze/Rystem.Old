using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem
{
    public class Retryable<T>
    {
        private readonly Func<Task<T>> Action;
        private Func<Exception, Task> OnError;
        private IRetryIntegration RetryIntegration;
        private readonly int MaxAttempts;
        private bool OnLastAttemptLaunchException;
        public Retryable(Func<Task<T>> action, int maxAttempts)
        {
            this.Action = action;
            this.MaxAttempts = maxAttempts;
        }
        public Retryable<T> WithCircuitBreak(string name = null)
            => this.WithCircuitBreak(100, TimeSpan.FromMinutes(5), name);
        public Retryable<T> WithCircuitBreak(int maxErrors, TimeSpan refreshWindows, string name = null, Func<CircuitBreakerLock, Task> lockEvent = null)
        {
            this.RetryIntegration = new CircuitBreakerRetry(this.MaxAttempts, name ?? this.GetType().FullName, maxErrors, refreshWindows, lockEvent);
            return this;
        }
        public Retryable<T> CatchError(Func<Exception, Task> onError)
        {
            this.OnError = onError;
            return this;
        }
        public Retryable<T> LaunchExceptionAfterLastAttempt()
        {
            this.OnLastAttemptLaunchException = true;
            return this;
        }
        public T Execute()
            => this.ExecuteAsync().ToResult();
        public async Task<T> ExecuteAsync()
        {
            if (this.RetryIntegration == null)
                this.RetryIntegration = new SimpleRetry(this.MaxAttempts);
            List<Exception> exceptions = null;
            do
            {
                try
                {
                    return await this.Action.Invoke().NoContext();
                }
                catch (Exception exception)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();
                    exceptions.Add(exception);
                    if (OnError != null)
                        await OnError.Invoke(exception).NoContext();
                }
            } while (this.RetryIntegration.IsRetryable(exceptions.FirstOrDefault()));
            if (OnLastAttemptLaunchException)
                throw new AggregateException(exceptions);
            else
                return default;
        }
    }
}