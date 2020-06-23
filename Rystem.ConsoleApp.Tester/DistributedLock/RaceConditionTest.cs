using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.DistributedLock
{
    public class RaceConditionTest : IUnitTest
    {
        static RaceCondition RaceCondition = new RaceCondition();
        private class MyNum
        {
            public int X { get; set; }
        }
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            MyNum myNum = new MyNum();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 30; i++)
                tasks.Add(Solitude(myNum));
            await Task.WhenAll(tasks);
            metrics.CheckIfOkExit(metrics.Command.NumberOfThread > 1 ? myNum.X == 1 || myNum.X == 0 : myNum.X == 1, myNum.X);
        }
        private static async Task Solitude(MyNum myNum)
        {
            await RaceCondition.ExecuteAsync(async () =>
            {
                await Task.Delay(4000);
                myNum.X++;
            }).NoContext();
        }
    }
}
