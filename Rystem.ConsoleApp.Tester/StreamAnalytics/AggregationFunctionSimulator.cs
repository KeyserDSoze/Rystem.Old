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
        private List<AbstractToParse> Things = new List<AbstractToParse>();
        public List<Exception> Exceptions = new List<Exception>();
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            MyCounter myCounter = new MyCounter();
            Aggregator aggregator = new Aggregator(myCounter);
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
            IList<AbstractToParse> flusheds = await aggregator.RunAsync(messages, null, async x => Things.Add(x), async (x, _) => Exceptions.Add(x));
            IList<AbstractToParse> flusheds2 = aggregator.Run(messages, null, x => Things.Add(x), (x, _) => Exceptions.Add(x), Installation.Inst00);
            await Task.Delay(10000);
            metrics.CheckIfOkExit(Things.Count == 220, Things.Count);
            metrics.CheckIfOkExit(myCounter.X == 5995, myCounter.X);
        }
        private abstract class AbstractToParse
        {
            public int X { get; set; }
            public override string ToString() => "Sample: " + this.X.ToString();
        }
        private class ObjectToParse : AbstractToParse
        {

        }
        private class ObjectToParse2 : AbstractToParse
        {

        }
        private class MyCounter
        {
            public int X { get; set; }
        }
        private class Aggregator : IAggregation<AbstractToParse>
        {
            private readonly MyCounter MyCounter;
            public Aggregator(MyCounter myCounter)
                => this.MyCounter = myCounter;
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithAggregation<AbstractToParse>().WithLinq(new LinqBuilder<AbstractToParse>("Alto", 80, TimeSpan.FromSeconds(30)))
                    .AddParser(new FunctionParser(this.MyCounter)).Build()
                    .WithAggregation<AbstractToParse>().WithLinq(new LinqBuilder<AbstractToParse>("Alto", 100, TimeSpan.FromSeconds(5)))
                    .AddParser(new FunctionParser(this.MyCounter)).Build(Installation.Inst00);
            }
        }

        private class FunctionParser : IAggregationParser<AbstractToParse>
        {
            public MyCounter Counter { get; }
            public FunctionParser(MyCounter myCounter)
            {
                this.Counter = myCounter;
            }
            public Task ParseAsync(string queueName, IList<AbstractToParse> events, ILogger log, Installation installation)
            {
                foreach (AbstractToParse a in events)
                    Counter.X += a.X;
                return Task.CompletedTask;
            }
        }
    }
}
