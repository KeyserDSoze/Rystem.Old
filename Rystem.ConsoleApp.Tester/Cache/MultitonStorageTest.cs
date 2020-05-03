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
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).NoContext();
            SmallTableKey smallTableKey = new SmallTableKey() { Id = 2 };
            smallTableKey.Remove();
            if (smallTableKey.IsPresent())
                return false;
            SmallTable smallTable = smallTableKey.Instance() as SmallTable;
            if (!smallTableKey.IsPresent())
                return false;
            smallTableKey.Restore(new SmallTable() { Id = 4 });
            if ((smallTableKey.Instance() as SmallTable).Id != 4)
                return false;
            if (smallTableKey.Keys().Count != 1)
                return false;
            if (!smallTableKey.Remove())
                return false;
            if (smallTableKey.IsPresent())
                return false;
            if (smallTableKey.Keys().Count != 0)
                return false;
            smallTable = smallTableKey.Instance() as SmallTable;
            Thread.Sleep(200);
            if (!smallTableKey.IsPresent())
                return false;
            Thread.Sleep(4800);
            if (smallTableKey.IsPresent())
                return false;
            smallTableKey.Restore(expiringTime: new TimeSpan(0, 0, 10));
            Thread.Sleep(7000);
            if (!smallTableKey.IsPresent())
                return false;
            Thread.Sleep(5000);
            if (smallTableKey.IsPresent())
                return false;
            return true;
        }
    }
    public class SmallTableKey : IMultitonKey<SmallTable>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static SmallTableKey()
        {
            MultitonInstaller.Configure<SmallTableKey, SmallTable>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.TableStorage, ExpireTime.FiveSeconds)));
        }
        public SmallTable Fetch()
        {
            return new SmallTable()
            {
                Id = this.Id
            };
        }
    }
    public class SmallTable : IMultiton
    {
        public int Id { get; set; }
    }
}
