using Rystem.Cache;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonHybridInMemoryAndCloud : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).NoContext();
            HybridTableKey hybridTableKey = new HybridTableKey() { Id = 2 };
            hybridTableKey.Remove();

            HybridTable hybridTable = hybridTableKey.Instance() as HybridTable;
            hybridTableKey.Restore(new HybridTable() { Id = 5 });
            hybridTableKey.Restore(new HybridTable() { Id = 7 });
            HybridTable secondHybridTable = new SecondHybridTableKey() { Id = 2 }.Instance();
            if (hybridTable.Id == secondHybridTable.Id)
                return false;
            return true;
        }
    }
    public class HybridTableKey : ICacheKey<HybridTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public Task<HybridTable> FetchAsync()
        {
            return Task.FromResult(new HybridTable()
            {
                Id = 5
            });
        }

        public ConfigurationBuilder GetConfigurationBuilder()
            => new ConfigurationBuilder()
                .WithCache()
                .WithMemory(new MemoryCacheProperties(ExpireTime.FiveMinutes, true))
                .And()
                .WithCloud(ConnectionString)
                .WithTablestorage(new TableStorageCacheProperties(ExpireTime.Infinite)).Build();
    }
    public class SecondHybridTableKey : ICacheKey<HybridTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public Task<HybridTable> FetchAsync()
        {
            return Task.FromResult(new HybridTable()
            {
                Id = this.Id
            });
        }
        public ConfigurationBuilder GetConfigurationBuilder()
            => new ConfigurationBuilder().WithCache()
                .WithMemory(new MemoryCacheProperties(ExpireTime.FiveSeconds, true))
                .And()
                .WithCloud(ConnectionString).WithTablestorage(new TableStorageCacheProperties(ExpireTime.Infinite))
                .Build();
    }
    public class HybridTable
    {
        public int Id { get; set; }
    }
}
