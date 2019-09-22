using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.ConsoleApp.Tester;
using Rystem.StreamAnalytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.StreamAnalytics
{
    public class FunctionSimulator : ITest
    {
        public bool DoWork(string entry)
        {
            AggregationInstaller<EventData>.Configure(
                new AggregationProperty()
                {
                    QueueName = "Alto",
                    MaximumBuffer = 4000,
                    Parsers = new List<IAggregationParser>() { new FunctionParser() }
                });
            AggregationManager<EventData> aggregationManager = new AggregationManager<EventData>();
            IList<EventData> messages = new List<EventData>();
            for (int i = 0; i < 5010; i++)
                messages.Add(
                    new EventData(
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new ObjectToParse()
                                {
                                    X = i
                                }))));
            EventData[] entries = messages.ToArray();
            aggregationManager.Run(entries, new Logger(), x => Console.WriteLine("action: " + x), x => Console.WriteLine("Action on error: " + x));
            return true;
        }
    }
    public class ObjectToParse
    {
        public int X { get; set; }
        public override string ToString() => "Sample: " + this.X.ToString();
    }
    public class FunctionParser : IAggregationParser
    {
        public void Parse<T>(string queueName, IList<T> events, ILogger log)
        {
            foreach (T a in events)
                log.LogInformation($"Parsing {a} in queue: {queueName}");
        }
    }
    public class Logger : ILogger, IDisposable
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
            return;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"{logLevel}: {state}");
        }
    }
}
