using Rystem;
using Rystem.Queue;
using Rystem.Debug;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class ServiceBusTester : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            MyServiceBus myServiceBus = new MyServiceBus()
            {
                A = "dsad",
                B = new MyObject()
                {
                    K = "dasdsa"
                }
            };
            List<MyServiceBus> myServiceBuses = new List<MyServiceBus>()
            {
                myServiceBus,
                myServiceBus
            };
            long messageId = await myServiceBus.SendScheduledAsync(120);
            metrics.CheckIfNotOkExit(messageId <= 0);
            bool returned = await myServiceBus.DeleteScheduledAsync(messageId);
            metrics.CheckIfNotOkExit(!returned);
            messageId = (await myServiceBuses.SendScheduledBatchAsync(120)).FirstOrDefault();
            metrics.CheckIfNotOkExit(messageId <= 0);
            returned = await myServiceBus.DeleteScheduledAsync(messageId);
            metrics.CheckIfNotOkExit(!returned);
            DebugMessage debugMessage = await myServiceBus.DebugSendAsync(120);
            metrics.CheckIfNotOkExit(!debugMessage.ServiceBusMessage.Contains("dsad"));
            DebugMessage debugMessage2 = await myServiceBuses.DebugSendBatchAsync(120);
            metrics.CheckIfNotOkExit(!debugMessage2.ServiceBusMessage.Contains("dsad"));
            messageId = await myServiceBus.SendScheduledAsync(120, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(messageId <= 0);
            returned = await myServiceBus.DeleteScheduledAsync(messageId, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!returned);
            messageId = (await myServiceBuses.SendScheduledBatchAsync(120, installation: Installation.Inst00)).FirstOrDefault();
            metrics.CheckIfNotOkExit(messageId <= 0);
            returned = await myServiceBus.DeleteScheduledAsync(messageId, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!returned);
            debugMessage = await myServiceBus.DebugSendAsync(120, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!debugMessage.ServiceBusMessage.Contains("dsad"));
            debugMessage2 = await myServiceBuses.DebugSendBatchAsync(120, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!debugMessage2.ServiceBusMessage.Contains("dsad"));
        }
        private class MyServiceBus : IQueue
        {
            public string A { get; set; }
            public MyObject B { get; set; }
            private const string ConnectionString = "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=";
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithQueue(ConnectionString)
                    .WithServiceBus(new ServiceBusBuilder("dario"))
                    .Build(Installation.Default)
                    .WithQueue(ConnectionString).WithServiceBus(new ServiceBusBuilder("aloa"))
                    .Build(Installation.Inst00);
            }
        }
        private class MyObject
        {
            public string K { get; set; }
        }
    }
}
