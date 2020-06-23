using Rystem.DistributedLock;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.DistributedLock
{
    public class DistributedRaceConditionTest : IUnitTest
    {
        private class MyNumber
        {
            public int X { get; set; }
        }
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            MyNumber myNum = new MyNumber();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(DistributedSingleton.ExecuteAsync(() => Go(myNum)));
            }
            await Task.WhenAll(tasks);
            metrics.CheckIfOkExit(metrics.Command.NumberOfThread > 1 ? myNum.X == 1 || myNum.X == 0 : myNum.X == 1, myNum.X);
        }
        private static async Task Go(MyNumber myNumber)
        {
            await Task.Delay(5000);
            myNumber.X++;
        }
        private static DistributedRaceCondition DistributedSingleton = new DistributedRaceCondition(TableStorageTester.ConnectionString, LockType.BlobStorage, "MyPensylvania");
    }
}
