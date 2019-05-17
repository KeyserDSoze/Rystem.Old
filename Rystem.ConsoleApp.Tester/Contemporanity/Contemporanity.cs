using Rystem.Azure.Queue;
using Rystem.Azure.Storage;
using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester
{
    public class Contemporanity
    {
        public static void Run()
        {
            CrossTest crossTest = new CrossTest();
            crossTest.Update();
        }
    }
    public class CrossTest : IServiceBus, ITableStorage, IMultiton
    {
        public string PartitionKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RowKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMultiton Fetch(IMultitonKey key)
        {
            throw new NotImplementedException();
        }
    }
}
