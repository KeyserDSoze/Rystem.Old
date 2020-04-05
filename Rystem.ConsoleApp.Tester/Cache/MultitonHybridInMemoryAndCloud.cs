using Rystem.Cache;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonHybridInMemoryAndCloud : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            HybridTableKey hybridTableKey = new HybridTableKey() { Id = 2 };
            hybridTableKey.Remove();

            HybridTable hybridTable = hybridTableKey.Instance() as HybridTable;
            Thread.Sleep(5000);
            hybridTable = hybridTableKey.Instance() as HybridTable;

            return true;
        }
    }
    public class HybridTableKey : IMultitonKey
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static HybridTableKey()
        {
            MultitonInstaller.Configure<HybridTableKey, HybridTable>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.TableStorage, ExpireTime.Infinite), new ExpiringProperties(ExpireTime.FiveSeconds)));
        }
        public IMultiton Fetch()
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
