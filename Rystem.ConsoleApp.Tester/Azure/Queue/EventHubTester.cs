using Rystem.Azure.Queue;
using Rystem.Debug;
using Rystem.Enums;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            MyAbstractionEventHub myEventHub = new MyEventHub()
            {
                A = "dsad",
                B = new MyObject()
                {
                    K = "dasdsa"
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
            myEventHub.DebugSend(0, installation: Installation.Inst00);
            myAbstractionEventHubs.DebugSendBatch(0, installation: Installation.Inst00);
            if (!myEventHub.Send(installation: Installation.Inst00))
                return false;
            if (!myAbstractionEventHubs.SendBatch(installation: Installation.Inst00))
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
            QueueInstaller.Configure<MyAbstractionEventHub>(new QueueConfiguration()
            {
                ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=",
                Name = "queue",
                Type = QueueType.EventHub
            });
            QueueInstaller.Configure<MyAbstractionEventHub>(new QueueConfiguration()
            {
                ConnectionString = "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=",
                Name = "aloa",
                Type = QueueType.EventHub
            }, Enums.Installation.Inst00);
        }  
    }
    public class MyObject
    {
        public string K { get; set; }
    }
}
