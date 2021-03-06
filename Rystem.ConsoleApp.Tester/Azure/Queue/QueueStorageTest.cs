using Azure.Storage.Queues;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.Queue
{
    public class QueueStorageTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            var client = new QueueServiceClient(KeyManager.Instance.Storage);
            var context = client.GetQueueClient("testone");
            await context.CreateIfNotExistsAsync();
            await context.SendMessageAsync("dsadsad");
            var t = await context.PeekMessagesAsync();
            var q = await context.ReceiveMessagesAsync();
            var z = await context.PeekMessagesAsync();
        }
    }
}