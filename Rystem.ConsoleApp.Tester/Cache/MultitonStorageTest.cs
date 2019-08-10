using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Cache
{
    public class MultitonTableStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            SmallTableKey smallTableKey = new SmallTableKey() { Id = 2 };
            smallTableKey.Remove();
            if (smallTableKey.IsPresent())
                return false;
            SmallTable smallTable = smallTableKey.Instance();
            if (!smallTableKey.IsPresent())
                return false;
            smallTableKey.Restore(new SmallTable() { Id = 4 });
            if (smallTableKey.Instance().Id != 4)
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
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=kynsexstorage;AccountKey=OCwrI4pGQtjc+HEfFetZ0TzExKfum2PrUfcao6cjQEyTfw1mJ15b2vNMWoBGYRkHsXwXJ/WqZXyy6BONehar+Q==;EndpointSuffix=core.windows.net";
        static SmallTableKey()
        {
            MultitonInstaller.Configure<SmallTableKey, SmallTable>(ConnectionString, InCloudType.TableStorage, CacheExpireTime.EightHour, MultitonExpireTime.TurnOff);
        }
    }
    public class SmallTable : IMultiton
    {
        public int Id { get; set; }
        public IMultiton Fetch(IMultitonKey key)
        {
            SmallTableKey smallBlobKey = (SmallTableKey)key;
            return new SmallTable()
            {
                Id = smallBlobKey.Id
            };
        }
    }
}
