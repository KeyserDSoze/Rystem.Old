using Rystem.Azure;
using Rystem.Azure.Queue;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            MyEventHub myEventHub = new MyEventHub()
            {
                A = "dsad",
                B = new MyObject()
                {
                    K = "dasdsa"
                }
            };
            List<MyEventHub> myAbstractionEventHubs = new List<MyEventHub>()
            {
                myEventHub,
                myEventHub
            };
            if (!await myEventHub.SendAsync())
                return false;
            if (!await myAbstractionEventHubs.SendBatchAsync())
                return false;
            await myEventHub.DebugSendAsync(0, installation: Installation.Inst00);
            await myAbstractionEventHubs.DebugSendBatchAsync(0, installation: Installation.Inst00);
            if (!await myEventHub.SendAsync(installation: Installation.Inst00))
                return false;
            if (!await myAbstractionEventHubs.SendBatchAsync(installation: Installation.Inst00))
                return false;
            return true;
        }
        private class MyEventHub : IQueue
        {
            public string A { get; set; }
            public MyObject B { get; set; }
            private const string ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=";
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithInstallation(Installation.Default)
                    .WithQueue(ConnectionString)
                    .WithEventHub(new RijndaelBuilder("queue"))
                        .Build()
                            .WithInstallation(Installation.Inst00)
                            .WithQueue(ConnectionString)
                            .WithEventHub(new RijndaelBuilder("aloa"))
                                .Build();
            }
        }
        private class MyObject
        {
            public string K { get; set; }
        }
    }
}
