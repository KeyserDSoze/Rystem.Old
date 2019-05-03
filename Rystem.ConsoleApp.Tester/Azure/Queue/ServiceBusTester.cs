using Rystem.Azure.Queue;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Azure.Queue
{
    public class ServiceBusTester : ITest
    {
        public bool DoWork(string entry)
        {
            MyServiceBus myServiceBus = new MyServiceBus()
            {
                A = "dsad",
                B = new MyServiceBus.MyObject()
                {
                    B = "dasdsa"
                }
            };
            long messageId = myServiceBus.Send(120).ConfigureAwait(false).GetAwaiter().GetResult();
            myServiceBus.Delete(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class MyServiceBus : IServiceBus
    {
        public string A { get; set; }
        public MyObject B { get; set; }
        static MyServiceBus()
        {
            ServiceBusHelper.Install<MyServiceBus>(
                "Endpoint=sb://kynsextest2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qxzmuricdYps0NufDo6rBaqMKVM1ZsgRo2htlfeItqw=");
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
