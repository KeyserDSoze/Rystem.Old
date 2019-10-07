﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Linq;
using Rystem.Enums;

namespace Rystem.StreamAnalytics
{
    public class AggregationManager<T>
    {
        private static Dictionary<Installation, object> TrafficLight = new Dictionary<Installation, object>();
        private static Dictionary<Installation, BufferBearer> Buffer = new Dictionary<Installation, BufferBearer>();
        private static readonly object AcquireToken = new object();
        private IDictionary<Installation, AggregationProperty> aggregationProperties;
        private IDictionary<Installation, AggregationProperty> AggregationProperties => aggregationProperties ?? (aggregationProperties = AggregationInstaller<T>.GetProperties());
        private string QueueName(Installation installation) => this.AggregationProperties[installation].Name;
        private class BufferBearer
        {
            public IList<T> Events { get; set; } = new List<T>();
            public long LastBufferCreation { get; set; } = DateTime.UtcNow.Ticks;
        }
        public IList<T> Run(IEnumerable<T> events, ILogger log, Action<T> action = null, Action<Exception, T> errorCatcher = null, Installation installation = Installation.Default)
        {
            this.CreateTrafficLight(installation);
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
                    this.Add(eventData, installation);
                }
                catch (Exception e)
                {
                    errorCatcher?.Invoke(e, eventData);
                    exceptions.Add(e);
                }
            }
            IList<T> flusheds = this.Flush(log, installation);
            DateTime endTime = DateTime.UtcNow;
            log.LogInformation($"istance: {istance} ends in {endTime} -> {endTime.Subtract(startTime).TotalSeconds} seconds. Number of events: {totalCount}. Number of errors: {exceptions.Count}. Example error:{exceptions.FirstOrDefault()}");
            return flusheds;
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
        public IList<T> Flush(ILogger log, Installation installation)
        {
            IList<T> events = new List<T>();
            log.LogWarning($"{this.QueueName(installation)}: {Buffer[installation].Events.Count} and {new DateTime(Buffer[installation].LastBufferCreation)}");
            DateTime startTime = DateTime.UtcNow;
            if (Buffer[installation].Events.Count > this.AggregationProperties[installation].MaximumBuffer || (Buffer[installation].Events.Count > 0 && startTime.Ticks - Buffer[installation].LastBufferCreation > this.AggregationProperties[installation].MaximumTime))
            {
                lock (TrafficLight[installation])
                {
                    if (Buffer[installation].Events.Count > this.AggregationProperties[installation].MaximumBuffer || (Buffer[installation].Events.Count > 0 && startTime.Ticks - Buffer[installation].LastBufferCreation > this.AggregationProperties[installation].MaximumTime))
                    {
                        foreach (T x in Buffer[installation].Events)
                            events.Add(x);
                        log.LogWarning($"Flushed {Buffer[installation].Events.Count} elements.");
                        Buffer[installation] = new BufferBearer();
                    }
                }
            }
            if (events.Count > 0)
            {
                foreach (IAggregationParser parser in this.AggregationProperties[installation].Parsers)
                {
                    try
                    {
                        parser.Parse(this.QueueName(installation), events, log, installation);
                        log.LogWarning($"Parsed {parser.GetType().Name}.");
                    }
                    catch (Exception er)
                    {
                        log.LogError(er.ToString());
                    }
                }
            }
            return events;
        }
    }
}
