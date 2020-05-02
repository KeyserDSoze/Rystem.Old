﻿using Rystem.Azure.Queue;
using Rystem.Debug;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class ServiceBusTester : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            MyServiceBus myServiceBus = new MyServiceBus()
            {
                A = "dsad",
                B = new MyObject()
                {
                    K = "dasdsa"
                }
            };
            List<MyServiceBus> myServiceBuses = new List<MyServiceBus>()
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
            messageId = myServiceBuses.SendScheduledBatch(120).FirstOrDefault();
            if (messageId <= 0)
                return false;
            returned = myServiceBus.DeleteScheduled(messageId);
            if (!returned)
                return false;
            DebugMessage debugMessage = myServiceBus.DebugSend(120);
            if (!debugMessage.ServiceBusMessage.Contains("dsad"))
                return false;
            DebugMessage debugMessage2 = myServiceBuses.DebugSendBatch(120);
            if (!debugMessage2.ServiceBusMessage.Contains("dsad"))
                return false;

            messageId = myServiceBus.SendScheduled(120, installation: Installation.Inst00);
            if (messageId <= 0)
                return false;
            returned = myServiceBus.DeleteScheduled(messageId, installation: Installation.Inst00);
            if (!returned)
                return false;
            messageId = myServiceBuses.SendScheduledBatch(120, installation: Installation.Inst00).FirstOrDefault();
            if (messageId <= 0)
                return false;
            returned = myServiceBus.DeleteScheduled(messageId, installation: Installation.Inst00);
            if (!returned)
                return false;
            debugMessage = myServiceBus.DebugSend(120, installation: Installation.Inst00);
            if (!debugMessage.ServiceBusMessage.Contains("dsad"))
                return false;
            debugMessage2 = myServiceBuses.DebugSendBatch(120, installation: Installation.Inst00);
            if (!debugMessage2.ServiceBusMessage.Contains("dsad"))
                return false;

            //ServiceBusMessage connectionMessage = new ServiceBusMessage()
            //{
            //    Attempt = 0,
            //    Container = myServiceBus,
            //    Flow =FlowType.Flow0,
            //    Version =VersionType.V0,
            //};
            //myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            //string jsonSent = connectionMessage.ToJson();
            //connectionMessage = jsonSent.ToServiceBusMessage();
            //myServiceBus = connectionMessage.ToObject<MyServiceBus>();
            //new MyServiceBus().Delete(4);
            //long aa = connectionMessage.SendFurther(30);
            return true;
        }
        private class MyServiceBus : IQueue
        {
            public string A { get; set; }
            public MyObject B { get; set; }
            static MyServiceBus()
            {
                QueueInstaller.Configure<MyServiceBus>(new QueueConfiguration()
                {
                    ConnectionString = "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=",
                    Type = QueueType.ServiceBus,
                    Name = "dario"
                });
                QueueInstaller.Configure<MyServiceBus>(new QueueConfiguration()
                {
                    ConnectionString = "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=",
                    Type = QueueType.ServiceBus,
                    Name = "aloa"
                }, Installation.Inst00);
            }
        }
        private class MyObject
        {
            public string K { get; set; }
        }
    }
}
