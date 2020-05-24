using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.StreamAnalytics
{
    public class FunctionSimulator : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Aggregator aggregator = new Aggregator();
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
            IList<EventData> flusheds = await aggregator.RunAsync(entries, new ConsoleLogger(), async x => Console.WriteLine("action: " + x), async (x, _) => Console.WriteLine("Action on error: " + x));
            IList<EventData> flusheds2 = aggregator.Run(entries, new ConsoleLogger(), x => Console.WriteLine("action2: " + x), (x, _) => Console.WriteLine("Action on error2: " + x), Installation.Inst00);
            return true;
        }
    }
    public class ObjectToParse
    {
        public int X { get; set; }
        public override string ToString() => "Sample: " + this.X.ToString();
    }
    public class Aggregator : IAggregation
    {
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithInstallation()
                .WithAggregation().WithLinq(new LinqBuilder("Alto", 80, TimeSpan.FromMinutes(5)))
                .AddParser(new FunctionParser()).Configure().Build()
                .WithInstallation(Installation.Inst00)
                .WithAggregation().WithLinq(new LinqBuilder("Alto", 100, TimeSpan.FromMinutes(5)))
                .AddParser(new FunctionParser()).Configure().Build();
        }
    }

    public class FunctionParser : IAggregationParser
    {
        public async Task ParseAsync<T>(string queueName, IList<T> events, ILogger log, Installation installation)
        {
            await Task.Delay(0).ConfigureAwait(false);
            foreach (T a in events)
                log.LogInformation($"Parsing {a} in queue: {queueName}");
        }
    }
}
