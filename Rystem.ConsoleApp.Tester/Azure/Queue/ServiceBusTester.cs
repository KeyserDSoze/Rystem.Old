using Rystem.Azure.Queue;
using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Azure.Queue
{
    public class ServiceBusTester : ITest
    {
        public bool DoWork(string entry)
        {
            MyAbstractionOfServiceBus myServiceBus = new MyServiceBus()
            {
                A = "dsad",
                B = new MyObject()
                {
                    B = "dasdsa"
                }
            };
            List<MyAbstractionOfServiceBus> myServiceBuses = new List<MyAbstractionOfServiceBus>()
            {
                myServiceBus,
                myServiceBus
            };
            long messageId = myServiceBus.Send(120);
            myServiceBus.DeleteScheduled(messageId);
            messageId = myServiceBuses.SendBatch(120);
            myServiceBus.DeleteScheduled(messageId);
            DebugMessage debugMessage = myServiceBus.DebugSend();
            DebugMessage debugMessage2 = myServiceBuses.DebugSendBatch();
            //ServiceBusMessage connectionMessage = new ServiceBusMessage()
            //{
            //    Attempt = 0,
            //    Container = myServiceBus,
            //    Flow = Enums.FlowType.Flow0,
            //    Version = Enums.VersionType.V0,
            //};
            //myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            //string jsonSent = connectionMessage.ToJson();
            //connectionMessage = jsonSent.ToServiceBusMessage();
            //myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            //new MyServiceBus().Delete(4);
            //long aa = connectionMessage.SendFurther(30);
            return true;
        }
    }
    public abstract class MyAbstractionOfServiceBus: IServiceBus
    {
        public string A { get; set; }
        public MyObject B { get; set; }
    }
    public class MyObject
    {
        public string B { get; set; }
    }
    public class MyServiceBus : MyAbstractionOfServiceBus
    {
        static MyServiceBus()
        {
            ServiceBusInstaller.Configure<MyServiceBus>(
                "Endpoint=sb://kynsextest2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qxzmuricdYps0NufDo6rBaqMKVM1ZsgRo2htlfeItqw=");
        }
       
    }
}
