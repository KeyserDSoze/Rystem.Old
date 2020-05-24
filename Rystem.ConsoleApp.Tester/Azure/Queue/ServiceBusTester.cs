using Rystem.Azure;
using Rystem.Azure.Queue;
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
            long messageId = await myServiceBus.SendScheduledAsync(120);
            if (messageId <= 0)
                return false;
            bool returned = await myServiceBus.DeleteScheduledAsync(messageId);
            if (!returned)
                return false;
            messageId = (await myServiceBuses.SendScheduledBatchAsync(120)).FirstOrDefault();
            if (messageId <= 0)
                return false;
            returned = await myServiceBus.DeleteScheduledAsync(messageId);
            if (!returned)
                return false;
            DebugMessage debugMessage = await myServiceBus.DebugSendAsync(120);
            if (!debugMessage.ServiceBusMessage.Contains("dsad"))
                return false;
            DebugMessage debugMessage2 = await myServiceBuses.DebugSendBatchAsync(120);
            if (!debugMessage2.ServiceBusMessage.Contains("dsad"))
                return false;

            messageId = await myServiceBus.SendScheduledAsync(120, installation: Installation.Inst00);
            if (messageId <= 0)
                return false;
            returned = await myServiceBus.DeleteScheduledAsync(messageId, installation: Installation.Inst00);
            if (!returned)
                return false;
            messageId = (await myServiceBuses.SendScheduledBatchAsync(120, installation: Installation.Inst00)).FirstOrDefault();
            if (messageId <= 0)
                return false;
            returned = await myServiceBus.DeleteScheduledAsync(messageId, installation: Installation.Inst00);
            if (!returned)
                return false;
            debugMessage = await myServiceBus.DebugSendAsync(120, installation: Installation.Inst00);
            if (!debugMessage.ServiceBusMessage.Contains("dsad"))
                return false;
            debugMessage2 = await myServiceBuses.DebugSendBatchAsync(120, installation: Installation.Inst00);
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
            private const string ConnectionString = "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=";
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder().WithInstallation(Installation.Default).WithQueue(ConnectionString)
                    .WithServiceBus(new ServiceBusBuilder("dario")).Build()
                    .WithInstallation(Installation.Inst00).WithQueue(ConnectionString).WithServiceBus(new ServiceBusBuilder("aloa"))
                    .Build();
            }
        }
        private class MyObject
        {
            public string K { get; set; }
        }
    }
}
