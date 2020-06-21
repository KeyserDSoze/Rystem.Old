using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    internal enum RetryFactor
    {
        Linear,
        Multiplicative,
        Exponential
    }
    public class Retryable<T>
    {
        private protected readonly Func<Task<T>> Action;
        private protected Func<Exception, Task> OnError;
        private protected IRetryIntegration RetryIntegration;
        private protected readonly int MaxAttempts;
        private protected bool AvoidLastAttemptThrowException;
        private protected IList<Exception> Exceptions;
        private protected RetryFactor RetryFactor;
        public Retryable(Func<Task<T>> action, int maxAttempts)
        {
            this.Action = action;
            this.MaxAttempts = maxAttempts;
        }
        public Retryable<T> WithMultiplicativeWaitAfterAnError()
        {
            this.RetryFactor = RetryFactor.Multiplicative;
            return this;
        }
        public Retryable<T> WithExponentialWaitAfterAnError()
        {
            this.RetryFactor = RetryFactor.Exponential;
            return this;
        }
        public Retryable<T> WithCircuitBreak(string name = null)
            => this.WithCircuitBreak(100, TimeSpan.FromMinutes(5), name);
        public Retryable<T> WithCircuitBreak(int maxErrors, TimeSpan refreshWindows, string name = null, Func<CircuitBreakerEvent, Task> lockEvent = null)
        {
            this.RetryIntegration = new CircuitBreakerRetry(this.MaxAttempts, name ?? this.GetType().FullName, maxErrors, refreshWindows, lockEvent);
            return this;
        }
        public Retryable<T> CatchError(Func<Exception, Task> onError)
        {
            this.OnError = onError;
            return this;
        }
        public Retryable<T> CatchError(IList<Exception> exceptions)
        {
            this.Exceptions = exceptions;
            return this;
        }
        public Retryable<T> NotThrowExceptionAfterLastAttempt()
        {
            this.AvoidLastAttemptThrowException = true;
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
                await WaitRightTime().NoContext();
                var result = await this.Action.TryCatchAsync(CatchException).NoContext();
                if (!result.InException)
                    return result.Result;
            } while (this.RetryIntegration.IsRetryable(exceptions.FirstOrDefault()));
            if (!AvoidLastAttemptThrowException)
                throw new AggregateException(exceptions);
            else
                return default;

            async Task CatchException(Exception exception)
            {
                if (exceptions == null)
                    exceptions = new List<Exception>();
                if (Exceptions != null)
                    Exceptions.Add(exception);
                exceptions.Add(exception);
                if (OnError != null)
                    await OnError.Invoke(exception).NoContext();
            }

            async Task WaitRightTime()
            {
                if (this.RetryIntegration.Attempts > 0)
                    switch (this.RetryFactor)
                    {
                        default:
                        case RetryFactor.Linear:
                            await Task.Delay(120).NoContext();
                            break;
                        case RetryFactor.Multiplicative:
                            await Task.Delay(this.RetryIntegration.Attempts * 120).NoContext();
                            break;
                        case RetryFactor.Exponential:
                            await Task.Delay((int)Math.Pow(15, this.RetryIntegration.Attempts)).NoContext();
                            break;
                    }
            }
        }
    }
}