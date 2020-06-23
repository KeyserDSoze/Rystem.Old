using Rystem.Cache;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonTableStorageTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            SmallTableKey smallTableKey = new SmallTableKey() { Id = 2 };
            smallTableKey.Remove();
            metrics.CheckIfNotOkExit(smallTableKey.IsPresent());
            SmallTable smallTable = smallTableKey.Instance() as SmallTable;
            metrics.CheckIfNotOkExit(!smallTableKey.IsPresent());
            smallTableKey.Restore(new SmallTable() { Id = 4 });
            metrics.CheckIfNotOkExit((smallTableKey.Instance() as SmallTable).Id != 4);
            metrics.CheckIfNotOkExit(smallTableKey.Keys().Count != 1);
            metrics.CheckIfNotOkExit(!smallTableKey.Remove());
            metrics.CheckIfNotOkExit(smallTableKey.IsPresent());
            metrics.CheckIfNotOkExit(smallTableKey.Keys().Count != 0);
            smallTable = smallTableKey.Instance() as SmallTable;
            await Task.Delay(200);
            metrics.CheckIfNotOkExit(!smallTableKey.IsPresent());
            await Task.Delay(4800);
            metrics.CheckIfNotOkExit(smallTableKey.IsPresent());
            smallTableKey.Restore(expiringTime: new TimeSpan(0, 0, 10));
            await Task.Delay(7000);
            metrics.CheckIfNotOkExit(!smallTableKey.IsPresent());
            await Task.Delay(5000);
            metrics.CheckIfNotOkExit(smallTableKey.IsPresent());
        }
    }
    public class SmallTableKey : ICacheKey<SmallTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";

        public Task<SmallTable> FetchAsync()
        {
            return Task.FromResult(new SmallTable()
            {
                Id = this.Id
            });
        }

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithCache().WithCloud(ConnectionString)
                .WithTablestorage(new TableStorageCacheProperties(ExpireTime.FiveSeconds)).Build();
        }
    }
    public class SmallTable
    {
        public int Id { get; set; }
    }
}
