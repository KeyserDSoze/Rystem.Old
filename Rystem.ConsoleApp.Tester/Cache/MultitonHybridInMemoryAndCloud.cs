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
            hybridTableKey.Restore(new HybridTable() { Id = 7 });
            HybridTable secondHybridTable = new SecondHybridTableKey() { Id = 2 }.Instance();
            if (hybridTable.Id == secondHybridTable.Id)
                return false;
            return true;
        }
    }
    public class HybridTableKey : IMultitonKey<HybridTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static HybridTableKey()
        {
            MultitonInstaller.Configure<HybridTableKey, HybridTable>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.TableStorage, ExpireTime.Infinite), new ExpiringProperties(ExpireTime.FiveSeconds)));
        }
        public HybridTable Fetch()
        {
            return new HybridTable()
            {
                Id = 5
            };
        }
    }
    public class SecondHybridTableKey : IMultitonKey<HybridTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static SecondHybridTableKey()
        {
            MultitonInstaller.Configure<SecondHybridTableKey, HybridTable>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.TableStorage, ExpireTime.Infinite), new ExpiringProperties(ExpireTime.FiveSeconds)));
        }
        public HybridTable Fetch()
        {
            return new HybridTable()
            {
                Id = this.Id
            };
        }
    }
    public class HybridTable : IMultiton
    {
        public int Id { get; set; }
    }
}
