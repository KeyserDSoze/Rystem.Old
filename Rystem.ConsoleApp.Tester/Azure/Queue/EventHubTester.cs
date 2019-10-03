using Rystem.Azure.Queue;
using Rystem.Debug;
using Rystem.Enums;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            MyAbstractionEventHub myEventHub = new MyEventHub()
            {
                A = "dsad",
                B = new MyEventHub.MyObject()
                {
                    B = "dasdsa"
                }
            };
            List<MyAbstractionEventHub> myAbstractionEventHubs = new List<MyAbstractionEventHub>()
            {
                myEventHub,
                myEventHub
            };
            if (!myEventHub.Send())
                return false;
            if (!myAbstractionEventHubs.SendBatch())
                return false;
            DebugMessage debugMessage = myEventHub.DebugSend(0, Installation.Inst00);
            DebugMessage debugMessages = myAbstractionEventHubs.DebugSendBatch(0, Installation.Inst00);
            if (!myEventHub.Send(Installation.Inst00))
                return false;
            if (!myAbstractionEventHubs.SendBatch(Installation.Inst00))
                return false;
            return true;
        }
    }
    public abstract class MyAbstractionEventHub : IQueue
    {

    }
    public class MyEventHub : MyAbstractionEventHub
    {
        public string A { get; set; }
        public MyObject B { get; set; }
        static MyEventHub()
        {
            QueueInstaller.Configure<MyEventHub>(new QueueConfiguration()
            {
                ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=",
                Name = "queue",
                Type = QueueType.EventHub
            });
            QueueInstaller.Configure<MyEventHub>(new QueueConfiguration()
            {
                ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=",
                Name = "aloa",
                Type = QueueType.EventHub
            }, Enums.Installation.Inst00);
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
