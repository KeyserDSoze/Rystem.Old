using Rystem.Cache;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonTableStorageTest : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
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
            if (smallTableKey.AllKeys().Count != 1)
                return false;
            if (!smallTableKey.Remove())
                return false;
            if (smallTableKey.IsPresent())
                return false;
            if (smallTableKey.AllKeys().Count != 0)
                return false;
            return true;
        }
    }
    public class SmallTableKey : IMultitonKey
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static SmallTableKey()
        {
            MultitonInstaller.Configure<SmallTableKey, SmallTable>(ConnectionString, InCloudType.TableStorage, CacheExpireTime.EightHour, MultitonExpireTime.TurnOff);
        }
        public IMultiton Fetch()
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
