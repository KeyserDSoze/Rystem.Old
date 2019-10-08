using Rystem.Azure.Queue;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class SmartQueueTester : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            MySmartQueue mySmartQueue = new MySmartQueue()
            {
                Al = "dsadsadsa",
                MyManagerImporter = new MyManagerImporter()
                {
                    A = "3",
                    B = 3,
                    C = 67
                }
            };
            long value = mySmartQueue.SendScheduled(0, 12, 23, Enums.Installation.Inst05);
            IEnumerable<MySmartQueue> xx = mySmartQueue.Read(installation: Enums.Installation.Inst05);
            if (xx.Count() <= 0)
                return false;
            xx = mySmartQueue.Read(12, installation: Enums.Installation.Inst05);
            if (xx.Count() == 1)
                return false;
            value = mySmartQueue.SendScheduled(0, 12, 23, Enums.Installation.Inst05);
            xx = mySmartQueue.Read(installation: Enums.Installation.Inst05, organization: 23);
            if (xx.Count() == 0)
                return false;
            int before = xx.Count();
            mySmartQueue.DeleteScheduled(value, installation: Enums.Installation.Inst05);
            xx = mySmartQueue.Read(installation: Enums.Installation.Inst05, organization: 23);
            if (xx.Count() == before)
                return false;
            IList<MySmartQueue> mySmartQueues = new List<MySmartQueue>();
            for (int i = 0; i < 50; i++)
                mySmartQueues.Add(mySmartQueue);
            mySmartQueues.SendScheduledBatch(0, 2, 3, Enums.Installation.Inst05);
            xx = mySmartQueue.Read(installation: Enums.Installation.Inst05, organization: 3);
            if (xx.Count() != 50)
                return false;
            xx = mySmartQueue.Read(installation: Enums.Installation.Inst05, organization: 3);
            if (xx.Count() > 0)
                return false;
            return true;
        }
    }
    public class MySmartQueue : IQueue
    {
        static MySmartQueue()
        {
            QueueInstaller.Configure<MySmartQueue>(new QueueConfiguration()
            {
                ConnectionString = "Server=tcp:myfirstservicebus.database.windows.net,1433;Initial Catalog=myfirstservicebus;Persist Security Info=False;User ID=myfirstservicebus;Password=DaveTheBeauty86;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Type = QueueType.SmartQueue,
                Name = "Business"
            }, Enums.Installation.Inst05);
        }
        public string Al { get; set; }
        public MyManagerImporter MyManagerImporter { get; set; }
    }
    public class MyManagerImporter
    {
        public string A { get; set; }
        public int B { get; set; }
        public double C { get; set; }
    }
}
