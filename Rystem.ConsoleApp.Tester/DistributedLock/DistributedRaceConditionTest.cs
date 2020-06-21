﻿using Rystem.DistributedLock;
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
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(DistributedSingleton.ExecuteAsync(Go));
            }
            await Task.WhenAll(tasks);
            return true;
        }
        public static async Task Go()
        {
            Console.WriteLine("Past");
            await Task.Delay(5000);
            Console.WriteLine("Present");
        }
        private static DistributedRaceCondition DistributedSingleton = new DistributedRaceCondition(TableStorageTester.ConnectionString, LockType.BlobStorage, "MyPensylvania");
    }
}