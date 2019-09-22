using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Rystem.StreamAnalytics
{
    public static class AggregationInstaller<T>
    {
        private static IDictionary<string, AggregationProperty> Installations = new Dictionary<string, AggregationProperty>();
        public static void Configure(AggregationProperty aggregationProperty)
        {
            Installations.Add(typeof(T).FullName, aggregationProperty);
        }
        public static AggregationProperty GetProperty()
        {
            return Installations[typeof(T).FullName];
        }
    }
    public class AggregationProperty
    {
        public string QueueName { get; set; }
        public int MaximumBuffer { get; set; } = 10000;
        public long MaximumTime { get; set; } = TimeSpan.FromMinutes(5).Ticks;
        public IList<IAggregationParser> Parsers { get; set; } = new List<IAggregationParser>();
    }
    public class AggregationManager<T>
    {
        private static string Name = typeof(T).Name;
        private static Dictionary<string, object> TrafficLight = new Dictionary<string, object>();
        private static Dictionary<string, BufferBearer> Buffer = new Dictionary<string, BufferBearer>();
        private static readonly object AcquireToken = new object();
        private AggregationProperty aggregationProperty;
        private AggregationProperty AggregationProperty => aggregationProperty ?? (aggregationProperty = AggregationInstaller<T>.GetProperty());
        private string QueueName => this.AggregationProperty.QueueName ?? Name;
        private class BufferBearer
        {
            public IList<T> Events { get; set; } = new List<T>();
            public long LastBufferCreation { get; set; } = DateTime.UtcNow.Ticks;
        }
        public void Run(IEnumerable<T> events, ILogger log, Action<T> action = null, Action<Exception> errorCatcher = null)
        {
            this.CreateTrafficLight();
            IList<Exception> exceptions = new List<Exception>();
            string istance = Guid.NewGuid().ToString("N");
            DateTime startTime = DateTime.UtcNow;
            log.LogInformation($"istance: {istance} -> {startTime}.");
            int totalCount = 0;
            foreach (T eventData in events)
            {
                try
                {
                    totalCount++;
                    action?.Invoke(eventData);
                    this.Add(eventData);
                }
                catch (Exception e)
                {
                    errorCatcher?.Invoke(e);
                    exceptions.Add(e);
                }
            }
            this.Flush(log);
            DateTime endTime = DateTime.UtcNow;
            log.LogInformation($"istance: {istance} throws error {endTime} -> {endTime.Subtract(startTime).TotalSeconds} seconds. Number of events: {totalCount}. Number of errors: {exceptions.Count}. Example error:{exceptions.FirstOrDefault()}");
        }
        private void CreateTrafficLight()
        {
            if (!TrafficLight.ContainsKey(this.AggregationProperty.QueueName))
                lock (AcquireToken)
                    if (!TrafficLight.ContainsKey(this.QueueName))
                    {
                        TrafficLight.Add(this.QueueName, new object());
                        Buffer.Add(this.QueueName, new BufferBearer());
                    }
        }
        private void Add(T singleEvent)
        {
            lock (TrafficLight[this.QueueName])
                Buffer[this.QueueName].Events.Add(singleEvent);
        }
        public void Flush(ILogger log)
        {
            log.LogDebug($"{this.QueueName}: {Buffer[this.QueueName].Events.Count} and {new DateTime(Buffer[this.QueueName].LastBufferCreation)}");
            DateTime startTime = DateTime.UtcNow;
            if (Buffer[this.QueueName].Events.Count > this.AggregationProperty.MaximumBuffer || (Buffer[this.QueueName].Events.Count > 0 && startTime.Ticks - Buffer[this.QueueName].LastBufferCreation > this.AggregationProperty.MaximumTime))
            {
                lock (TrafficLight[this.QueueName])
                {
                    if (Buffer[this.QueueName].Events.Count > this.AggregationProperty.MaximumBuffer || (Buffer[this.QueueName].Events.Count > 0 && startTime.Ticks - Buffer[this.QueueName].LastBufferCreation > this.AggregationProperty.MaximumTime))
                    {
                        foreach (IAggregationParser parser in this.AggregationProperty.Parsers)
                        {
                            try
                            {
                                parser.Parse(this.QueueName, Buffer[this.QueueName].Events, log);
                            }
                            catch (Exception er)
                            {
                                log.LogError(er.ToString());
                            }
                        }
                        log.LogWarning($"Flushed {Buffer[this.QueueName].Events.Count} elements.");
                        Buffer[this.QueueName] = new BufferBearer();
                    }
                }
            }
        }
    }
}
