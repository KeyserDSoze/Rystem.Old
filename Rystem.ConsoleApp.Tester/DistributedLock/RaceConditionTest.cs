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
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 30; i++)
                tasks.Add(Solitude());
            await Task.WhenAll(tasks);
            return true;
        }
        private static async Task Solitude()
        {
            await RaceCondition.ExecuteAsync(async () =>
            {
                Console.WriteLine("Start to");
                await Task.Delay(4000);
                Console.WriteLine("End to");
            });
        }
    }
}
