﻿using Rystem.Azure.Queue;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class SmartQueueTester : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
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
            long value = await mySmartQueue.SendScheduledAsync(0, 12, 23, Installation.Inst05);
            IEnumerable<MySmartQueue> xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05);
            if (xx.Count() <= 0)
                return false;
            xx = await mySmartQueue.ReadAsync(12, installation: Installation.Inst05);
            if (xx.Count() == 1)
                return false;
            value = await mySmartQueue.SendScheduledAsync(0, 12, 23, Installation.Inst05);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 23);
            if (xx.Count() == 0)
                return false;
            int before = xx.Count();
            await mySmartQueue.DeleteScheduledAsync(value, installation: Installation.Inst05);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 23);
            if (xx.Count() == before)
                return false;
            IList<MySmartQueue> mySmartQueues = new List<MySmartQueue>();
            for (int i = 0; i < 50; i++)
                mySmartQueues.Add(mySmartQueue);
            await mySmartQueues.SendScheduledBatchAsync(0, 2, 3, Installation.Inst05);
            xx = mySmartQueue.Read(installation: Installation.Inst05, organization: 3);
            if (xx.Count() != 50)
                return false;
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 3);
            if (xx.Count() > 0)
                return false;

            //Duplication check on path
            long v1 = await mySmartQueue.SendScheduledAsync(0, 1, 23);
            long v2 = await mySmartQueue.SendScheduledAsync(0, 1, 23);
            if (v1 == v2)
                return false;
            if (v2 > 0)
                return false;

            xx = await mySmartQueue.ReadAsync(1, 23);
            if (xx.Count() != 1)
                return false;
            xx = await mySmartQueue.ReadAsync(1, 23);
            if (xx.Count() > 0)
                return false;

            //Duplication check on message
            v1 = await mySmartQueue.SendScheduledAsync(0, installation: Installation.Inst00);
            v2 = await mySmartQueue.SendScheduledAsync(0, installation: Installation.Inst00);
            if (v1 == v2)
                return false;
            if (v2 > 0)
                return false;

            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst00);
            if (xx.Count() != 1)
                return false;
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst00);
            if (xx.Count() > 0)
                return false;

            //Duplication check on message and path
            v1 = await mySmartQueue.SendScheduledAsync(0, 1, 23, installation: Installation.Inst01);
            v2 = await mySmartQueue.SendScheduledAsync(0, 1, 23, installation: Installation.Inst01);
            if (v1 == v2)
                return false;
            if (v2 > 0)
                return false;

            xx = await mySmartQueue.ReadAsync(1, 23, installation: Installation.Inst01);
            if (xx.Count() != 1)
                return false;
            xx = await mySmartQueue.ReadAsync(1, 23, installation: Installation.Inst01);
            if (xx.Count() > 0)
                return false;

            bool va = await mySmartQueue.CleanAsync(Installation.Inst01);
            if (!va)
                return false;

            for (int attempt = 0; attempt < 2; attempt++)
                va = await mySmartQueue.CleanAsync(Installation.Inst05);

            if (va)
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
                ConnectionString = "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Type = QueueType.SmartQueue,
                Name = "Business",
                Retry = 3,
            }, Installation.Inst05);
            QueueInstaller.Configure<MySmartQueue>(new QueueConfiguration()
            {
                ConnectionString = "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Type = QueueType.SmartQueue,
                Name = "Notification",
                CheckDuplication = QueueDuplication.Path,
                Retry = 3
            });
            QueueInstaller.Configure<MySmartQueue>(new QueueConfiguration()
            {
                ConnectionString = "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Type = QueueType.SmartQueue,
                Name = "Unsubscription",
                CheckDuplication = QueueDuplication.Message,
                Retry = 3
            }, Installation.Inst00);
            QueueInstaller.Configure<MySmartQueue>(new QueueConfiguration()
            {
                ConnectionString = "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Type = QueueType.SmartQueue,
                Name = "Billing",
                CheckDuplication = QueueDuplication.PathAndMessage,
                Retry = 3,
                Retention = -1
            }, Installation.Inst01);
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
