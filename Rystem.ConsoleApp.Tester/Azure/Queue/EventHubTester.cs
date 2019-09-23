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
            myEventHub.Send();
            myAbstractionEventHubs.SendBatch();
            DebugMessage debugMessage = myEventHub.DebugSend();
            DebugMessage debugMessages = myAbstractionEventHubs.DebugSendBatch();
            //EventHubMessage connectionMessage = new EventHubMessage()
            //{
            //    Attempt = 0,
            //    Container = myEventHub,
            //    Flow = Enums.FlowType.Flow0,
            //    Version = Enums.VersionType.V0,
            //};
            //myEventHub = connectionMessage.ToObject<MyEventHub>();
            //string jsonSent = connectionMessage.ToJson();
            //connectionMessage = jsonSent.ToEventHubMessage();
            //myEventHub = connectionMessage.ToObject<MyEventHub>();
            //connectionMessage.SendFurther(30);
            return true;
        }
    }
    public abstract class MyAbstractionEventHub : IQueueMessage
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
                ConnectionString = "Endpoint=sb://kynsextest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=9CAVSQGOLEmlKziA3xmK4mM6Oc6SOLCQ+FBmzVL1+54="
            });
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
