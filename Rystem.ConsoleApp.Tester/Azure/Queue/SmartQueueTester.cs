using Rystem;
using Rystem.Queue;
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
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
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
            metrics.CheckIfNotOkExit(xx.Count() <= 0);
            xx = await mySmartQueue.ReadAsync(12, installation: Installation.Inst05);
            metrics.CheckIfNotOkExit(xx.Count() == 1);
            value = await mySmartQueue.SendScheduledAsync(0, 12, 23, Installation.Inst05);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 23);
            metrics.CheckIfNotOkExit(xx.Count() == 0);
            int before = xx.Count();
            await mySmartQueue.DeleteScheduledAsync(value, installation: Installation.Inst05);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 23);
            metrics.CheckIfNotOkExit(xx.Count() == before);
            IList<MySmartQueue> mySmartQueues = new List<MySmartQueue>();
            for (int i = 0; i < 50; i++)
                mySmartQueues.Add(mySmartQueue);
            await mySmartQueues.SendScheduledBatchAsync(0, 2, 3, Installation.Inst05);
            xx = mySmartQueue.Read(installation: Installation.Inst05, organization: 3);
            metrics.CheckIfNotOkExit(xx.Count() != 50);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst05, organization: 3);
            metrics.CheckIfNotOkExit(xx.Count() > 0);
            //Duplication check on path
            long v1 = await mySmartQueue.SendScheduledAsync(0, 1, 23);
            long v2 = await mySmartQueue.SendScheduledAsync(0, 1, 23);
            metrics.CheckIfNotOkExit(v1 == v2);
            metrics.CheckIfNotOkExit(v2 > 0);

            xx = await mySmartQueue.ReadAsync(1, 23);
            metrics.CheckIfNotOkExit(xx.Count() != 1);
            xx = await mySmartQueue.ReadAsync(1, 23);
            metrics.CheckIfNotOkExit(xx.Count() > 0);

            //Duplication check on message
            v1 = await mySmartQueue.SendScheduledAsync(0, installation: Installation.Inst00);
            v2 = await mySmartQueue.SendScheduledAsync(0, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(v1 == v2);
            metrics.CheckIfNotOkExit(v2 > 0);

            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(xx.Count() != 1);
            xx = await mySmartQueue.ReadAsync(installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(xx.Count() > 0);

            //Duplication check on message and path
            v1 = await mySmartQueue.SendScheduledAsync(0, 1, 23, installation: Installation.Inst01);
            v2 = await mySmartQueue.SendScheduledAsync(0, 1, 23, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(v1 == v2);
            metrics.CheckIfNotOkExit(v2 > 0);

            xx = await mySmartQueue.ReadAsync(1, 23, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(xx.Count() != 1);
            xx = await mySmartQueue.ReadAsync(1, 23, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(xx.Count() > 0);

            bool va = await mySmartQueue.CleanAsync(Installation.Inst01);
            metrics.CheckIfNotOkExit(!va);
            for (int attempt = 0; attempt < 2; attempt++)
                va = await mySmartQueue.CleanAsync(Installation.Inst05);
            metrics.CheckIfNotOkExit(va);
        }
    }
    public class MySmartQueue : IQueue
    {
        private const string ConnectionString = "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public string Al { get; set; }
        public MyManagerImporter MyManagerImporter { get; set; }

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .WithQueue(ConnectionString)
                .WithSmartQueue(new SmartQueueBuilder("Billing", QueueDuplication.PathAndMessage, 100, 3, -1))
                .Build(Installation.Inst01)
                .WithQueue(ConnectionString)
                .WithSmartQueue(new SmartQueueBuilder("Unsubscription", QueueDuplication.Message, 100, 3))
                .Build(Installation.Inst00)
                .WithQueue(ConnectionString)
                .WithSmartQueue(new SmartQueueBuilder("Notification", QueueDuplication.Path, 100, 3, 30))
                .Build(Installation.Default)
                .WithQueue(ConnectionString)
                .WithSmartQueue(new SmartQueueBuilder("Business", QueueDuplication.Allow, 100, 3, 30))
                .Build(Installation.Inst05);
        }
    }
    public class MyManagerImporter
    {
        public string A { get; set; }
        public int B { get; set; }
        public double C { get; set; }
    }
}
