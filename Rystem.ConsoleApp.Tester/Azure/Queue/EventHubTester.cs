﻿using Rystem;
using Rystem.Queue;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
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
            metrics.CheckIfNotOkExit(!await myEventHub.SendAsync());
            metrics.CheckIfNotOkExit(!await myAbstractionEventHubs.SendBatchAsync());
            await myEventHub.DebugSendAsync(0, installation: Installation.Inst00);
            await myAbstractionEventHubs.DebugSendBatchAsync(0, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!await myEventHub.SendAsync(installation: Installation.Inst00));
            metrics.CheckIfNotOkExit(!await myAbstractionEventHubs.SendBatchAsync(installation: Installation.Inst00));
        }
        private class MyEventHub : IQueue
        {
            public string A { get; set; }
            public MyObject B { get; set; }
            private const string ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=";
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithQueue(ConnectionString)
                    .WithEventHub(new EventHubBuilder("queue"))
                        .Build(Installation.Default)
                            .WithQueue(ConnectionString)
                            .WithEventHub(new EventHubBuilder("aloa"))
                                .Build(Installation.Inst00);
            }
        }
        private class MyObject
        {
            public string K { get; set; }
        }
    }
}
