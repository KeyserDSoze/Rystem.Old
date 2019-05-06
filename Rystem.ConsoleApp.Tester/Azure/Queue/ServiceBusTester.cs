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
            //long messageId = myServiceBus.Send(120).ConfigureAwait(false).GetAwaiter().GetResult();
            //myServiceBus.Delete(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
            ServiceBusMessage connectionMessage = new ServiceBusMessage()
            {
                Attempt = 0,
                Container = myServiceBus,
                Flow = Enums.FlowType.Flow0,
                Version = Enums.VersionType.V0,
            };
            myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            string jsonSent = connectionMessage.ToJson();
            connectionMessage = jsonSent.ToServiceBusMessage();
            myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            new MyServiceBus().Delete(4);
            long aa = connectionMessage.SendFurther(30);
            return true;
        }
    }
    public class MyServiceBus : IServiceBus
    {
        public string A { get; set; }
        public MyObject B { get; set; }
        static MyServiceBus()
        {
            ServiceBusInstaller.Configure<MyServiceBus>(
                "Endpoint=sb://kynsextest2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qxzmuricdYps0NufDo6rBaqMKVM1ZsgRo2htlfeItqw=");
        }
        public class MyObject
        {
            public string B { get; set; }
        }
    }
}
