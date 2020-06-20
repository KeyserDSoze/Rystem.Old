using Microsoft.Extensions.Logging;
using Rystem;
using Rystem.Aggregation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System
{
    public class CircuitBreakerEvent
    {
        public bool Lock { get; }
        public IList<Exception> Exceptions { get; }
        internal CircuitBreakerEvent(bool @lock, IList<Exception> exceptions)
        {
            this.Lock = @lock;
            this.Exceptions = exceptions;
        }
        public override string ToString()
            => $"Lock: {Lock}, Exceptions: {Exceptions.Count}";
    }
    internal class CircuitBreakerRetry : IRetryIntegration
    {
        private readonly int MaxAttempts;
        private int Attempts;
        private readonly string Name;
        private static readonly ConcurrentDictionary<string, CircuitBreakerAggregator> Aggregators = new ConcurrentDictionary<string, CircuitBreakerAggregator>();
        private readonly int MaxErrors;
        public CircuitBreakerRetry(int maxAttempts, string name, int maxErrors, TimeSpan maximumTime, Func<CircuitBreakerEvent, Task> circuitBreakerEvent)
        {
            this.MaxAttempts = maxAttempts;
            this.Name = name;
            if (!Aggregators.ContainsKey(this.Name))
                Aggregators.TryAdd(this.Name, new CircuitBreakerAggregator(this, maximumTime, circuitBreakerEvent));
            this.MaxErrors = maxErrors;
        }
        public bool IsRetryable(Exception exception)
        {
            if (exception == null)
                return false;
            CircuitBreakerAggregator aggregator = Aggregators[this.Name];
            aggregator.Run(exception);
            if (aggregator.IsLocked)
                return false;
            this.Attempts++;
            return this.Attempts < this.MaxAttempts;
        }
        private class CircuitBreakerAggregator : IAggregation<Exception>
        {
            public CircuitBreakerRetry CircuitBreakerRetry { get; }
            private readonly TimeSpan MaximumTime;
            public bool IsLocked { get; internal set; }
            public Func<CircuitBreakerEvent, Task> LockEvent { get; }
            public CircuitBreakerAggregator(CircuitBreakerRetry circuitBreakerRetry, TimeSpan maximumTime, Func<CircuitBreakerEvent, Task> lockEvent)
            {
                this.CircuitBreakerRetry = circuitBreakerRetry;
                this.MaximumTime = maximumTime;
                this.LockEvent = lockEvent;
            }

            public ConfigurationBuilder GetConfigurationBuilder()
                => new ConfigurationBuilder()
                    .WithAggregation<Exception>()
                        .WithLinq(new LinqBuilder<Exception>(this.CircuitBreakerRetry.Name, this.CircuitBreakerRetry.MaxErrors, this.MaximumTime))
                            .AddParser(new AggregationParser(this))
                            .Build();
        }
        private class AggregationParser : IAggregationParser<Exception>
        {
            private readonly CircuitBreakerAggregator Aggregator;
            public AggregationParser(CircuitBreakerAggregator aggregator)
                => this.Aggregator = aggregator;
            public async Task ParseAsync(string queueName, IList<Exception> events, ILogger log, Installation installation)
            {
                if (events.Count > this.Aggregator.CircuitBreakerRetry.MaxErrors)
                    this.Aggregator.IsLocked = true;
                else
                    this.Aggregator.IsLocked = false;
                if (this.Aggregator.LockEvent != null)
                    await this.Aggregator.LockEvent.Invoke(new CircuitBreakerEvent(this.Aggregator.IsLocked, events));
            }
        }
    }
}