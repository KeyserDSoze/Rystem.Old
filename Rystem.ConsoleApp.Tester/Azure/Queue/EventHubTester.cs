using Rystem.Azure.Queue;
using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : ITest
    {
        public bool DoWork(string entry)
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
            DebugMessage debugMessage = myEventHub.DebugSend();
            DebugMessage debugMessages = myAbstractionEventHubs.DebugSendBatch();

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
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
