using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Rystem;
using Rystem.Fast;
using System.Threading;

namespace Rystem.Aggregation
{
    internal class AggregationManager<T> : IAggregationManager<T>
    {
        private readonly static Dictionary<Installation, object> TrafficLight = new Dictionary<Installation, object>();
        private readonly static Dictionary<Installation, BufferBearer> Buffer = new Dictionary<Installation, BufferBearer>();
        private static readonly object AcquireToken = new object();
        private readonly IDictionary<Installation, AggregationConfiguration<T>> AggregationProperties;
        public InstallerType InstallerType => InstallerType.Aggregation;
        private string AggregationName(Installation installation)
            => this.AggregationProperties[installation].Name;
        public TimeSpan GetAggregationTime(Installation installation)
            => new TimeSpan(this.AggregationProperties[installation].MaximumTime);
        public IEnumerable<Installation> GetInstallations()
            => this.AggregationProperties.Select(x => x.Key);
        public AggregationManager(ConfigurationBuilder configurationBuilder)
        {
            this.AggregationProperties = configurationBuilder.GetConfigurations(this.InstallerType).ToDictionary(x => x.Key, x => x.Value as AggregationConfiguration<T>);
            foreach (var installation in AggregationProperties.Keys)
                this.CreateTrafficLight(installation);
        }
        public async Task<IList<T>> RunAsync(IEnumerable<T> events, ILogger log, Func<T, Task> action = null, Func<Exception, T, Task> errorCatcher = null, Installation installation = Installation.Default)
        {
            IList<Exception> exceptions = new List<Exception>();
            string instance = Guid.NewGuid().ToString("N");
            DateTime startTime = DateTime.UtcNow;
            log?.LogInformation($"instance: {instance} -> {startTime}.");
            int totalCount = 0;
            foreach (T eventData in events)
            {
                try
                {
                    totalCount++;
                    if (action != null)
                        await action.Invoke(eventData);
                    this.Add(eventData, installation);
                }
                catch (Exception e)
                {
                    if (errorCatcher != null)
                        await errorCatcher.Invoke(e, eventData);
                    exceptions.Add(e);
                }
            }
            IList<T> flusheds = await this.FlushAsync(log, installation).NoContext();
            DateTime endTime = DateTime.UtcNow;
            log?.LogInformation($"instance: {instance} ends in {endTime} -> {endTime.Subtract(startTime).TotalSeconds} seconds. Number of events: {totalCount}. Number of errors: {exceptions.Count}. Example error:{exceptions.FirstOrDefault()}");
            return flusheds;
        }
        public async Task AutoFlushAsync(Installation installation)
            => await this.FlushAsync(null, installation).NoContext();
        public Task<IList<T>> FlushAsync(ILogger log, Installation installation)
        {
            IList<T> events = new List<T>();
            log?.LogWarning($"{this.AggregationName(installation)}: {Buffer[installation].Events.Count} and {new DateTime(Buffer[installation].LastBufferCreation)}");
            DateTime startTime = DateTime.UtcNow;
            if (Buffer[installation].Events.Count > this.AggregationProperties[installation].MaximumBuffer || (Buffer[installation].Events.Count > 0 && startTime.Ticks - Buffer[installation].LastBufferCreation > this.AggregationProperties[installation].MaximumTime))
            {
                lock (TrafficLight[installation])
                {
                    if (Buffer[installation].Events.Count > this.AggregationProperties[installation].MaximumBuffer || (Buffer[installation].Events.Count > 0 && startTime.Ticks - Buffer[installation].LastBufferCreation > this.AggregationProperties[installation].MaximumTime))
                    {
                        foreach (T x in Buffer[installation].Events)
                            events.Add(x);
                        log?.LogWarning($"Flushed {Buffer[installation].Events.Count} elements.");
                        Buffer[installation] = new BufferBearer();
                    }
                }
            }
            if (events.Count > 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(async (context) =>
                {
                    foreach (IAggregationParser<T> parser in this.AggregationProperties[installation].Parsers.Select(x => x))
                    {
                        try
                        {
                            await parser.ParseAsync(this.AggregationName(installation), events, log, installation).NoContext();
                            log?.LogWarning($"Parsed {parser.GetType().Name}.");
                        }
                        catch (Exception er)
                        {
                            log?.LogError(er.ToString());
                        }
                    }
                }, EmptyObject);
            }
            return Task.FromResult(events);
        }
        private static readonly object EmptyObject = new object();
        private class BufferBearer
        {
            public IList<T> Events { get; set; } = new List<T>();
            public long LastBufferCreation { get; set; } = DateTime.UtcNow.Ticks;
        }
        private void CreateTrafficLight(Installation installation)
        {
            if (!TrafficLight.ContainsKey(installation))
                lock (AcquireToken)
                    if (!TrafficLight.ContainsKey(installation))
                    {
                        TrafficLight.Add(installation, new object());
                        Buffer.Add(installation, new BufferBearer());
                    }
        }
        private void Add(T singleEvent, Installation installation)
        {
            lock (TrafficLight[installation])
                Buffer[installation].Events.Add(singleEvent);
        }
    }
}
