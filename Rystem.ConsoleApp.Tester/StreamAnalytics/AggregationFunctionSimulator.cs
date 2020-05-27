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
            IList<AbstractToParse> messages = new List<AbstractToParse>();
            for (int i = 0; i < 50; i++)
                messages.Add(
                                new ObjectToParse()
                                {
                                    X = i
                                });
            for (int i = 50; i < 110; i++)
                messages.Add(
                                new ObjectToParse2()
                                {
                                    X = i
                                });
            IList<AbstractToParse> flusheds = await aggregator.RunAsync(messages, new ConsoleLogger(), async x => Console.WriteLine("action: " + x), async (x, _) => Console.WriteLine("Action on error: " + x));
            IList<AbstractToParse> flusheds2 = aggregator.Run(messages, new ConsoleLogger(), x => Console.WriteLine("action2: " + x), (x, _) => Console.WriteLine("Action on error2: " + x), Installation.Inst00);
            return true;
        }
    }
    public abstract class AbstractToParse
    {
        public int X { get; set; }
        public override string ToString() => "Sample: " + this.X.ToString();
    }
    public class ObjectToParse : AbstractToParse
    {
        
    }
    public class ObjectToParse2 : AbstractToParse
    {

    }
    public class Aggregator : IAggregation<AbstractToParse>
    {
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .WithAggregation<AbstractToParse>().WithLinq(new LinqBuilder<AbstractToParse>("Alto", 80, TimeSpan.FromMinutes(5)))
                .AddParser(new FunctionParser()).Configure().Build()
                .WithAggregation<AbstractToParse>().WithLinq(new LinqBuilder<AbstractToParse>("Alto", 100, TimeSpan.FromMinutes(5)))
                .AddParser(new FunctionParser()).Configure().Build(Installation.Inst00);
        }
    }

    public class FunctionParser : IAggregationParser<AbstractToParse>
    {
        public async Task ParseAsync(string queueName, IList<AbstractToParse> events, ILogger log, Installation installation)
        {
            await Task.Delay(0).ConfigureAwait(false);
            int count = 0;
            foreach (AbstractToParse a in events)
                count += a.X;
            Console.WriteLine(count);
        }
    }
}
