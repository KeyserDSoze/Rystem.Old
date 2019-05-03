using Rystem.Azure.Queue;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Azure.Queue
{
    public class EventHubTester : ITest
    {
        public bool DoWork(string entry)
        {
            MyEventHub myEventHub = new MyEventHub()
            {
                A = "dsad",
                B = new MyEventHub.MyObject()
                {
                    B = "dasdsa"
                }
            };
            myEventHub.Send().ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class MyEventHub : IEventHub
    {
        public string A { get; set; }
        public MyObject B { get; set; }
        static MyEventHub()
        {
            EventHubHelper.Install<MyEventHub>(
                "Endpoint=sb://kynsextest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=9CAVSQGOLEmlKziA3xmK4mM6Oc6SOLCQ+FBmzVL1+54=");
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
