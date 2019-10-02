using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.ConsoleApp.Tester;
using Rystem.Enums;
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
                    Name = "Alto",
                    MaximumBuffer = 80,
                    Parsers = new List<IAggregationParser>() { new FunctionParser() }
                });
            AggregationInstaller<EventData>.Configure(
                new AggregationProperty()
                {
                    Name = "Alto2",
                    MaximumBuffer = 100,
                    Parsers = new List<IAggregationParser>() { new FunctionParser() }
                }, Installation.Inst00);
            AggregationManager<EventData> aggregationManager = new AggregationManager<EventData>();
            IList<EventData> messages = new List<EventData>();
            for (int i = 0; i < 110; i++)
                messages.Add(
                    new EventData(
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new ObjectToParse()
                                {
                                    X = i
                                }))));
            EventData[] entries = messages.ToArray();
            IList<EventData> flusheds = aggregationManager.Run(entries, new Logger(), x => Console.WriteLine("action: " + x), (x, _) => Console.WriteLine("Action on error: " + x));
            IList<EventData> flusheds2 = aggregationManager.Run(entries, new Logger(), x => Console.WriteLine("action2: " + x), (x, _) => Console.WriteLine("Action on error2: " + x), Installation.Inst00);
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
        public void Parse<T>(string queueName, IList<T> events, ILogger log, Installation installation)
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
