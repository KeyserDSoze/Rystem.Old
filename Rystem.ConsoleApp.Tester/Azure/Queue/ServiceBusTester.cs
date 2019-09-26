using Rystem.Azure.Queue;
using Rystem.Debug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
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
            long messageId = myServiceBus.SendScheduled(120);
            if (messageId <= 0)
                return false;
            bool returned = myServiceBus.DeleteScheduled(messageId);
            if (!returned)
                return false;
            messageId = myServiceBuses.SendScheduledBatch(120)[0];
            if (messageId <= 0)
                return false;
            returned = myServiceBus.DeleteScheduled(messageId);
            if (!returned)
                return false;
            DebugMessage debugMessage = myServiceBus.DebugSend();
            if (!debugMessage.ServiceBusMessage.Contains("dsad"))
                return false;
            DebugMessage debugMessage2 = myServiceBuses.DebugSendBatch();
            if (!debugMessage2.ServiceBusMessage.Contains("dsad"))
                return false;
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
    public abstract class MyAbstractionOfServiceBus : IQueue
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
            QueueInstaller.Configure<MyServiceBus>(new QueueConfiguration()
            {
                ConnectionString = "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=",
                Type = QueueType.ServiceBus,
                Name = "dario"
            });
        }

    }
}
