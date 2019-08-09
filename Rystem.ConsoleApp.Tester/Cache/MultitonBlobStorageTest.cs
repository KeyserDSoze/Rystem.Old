using Rystem.Cache;
using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonBlobStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            SmallBlobKey smallBlobKey = new SmallBlobKey() { Id = 2 };
            if (smallBlobKey.IsPresent())
                return false;
            SmallBlob smallBlob = smallBlobKey.Instance();
            if (!smallBlobKey.IsPresent())
                return false;
            smallBlobKey.Restore(new SmallBlob() { Id = 4 });
            if (smallBlobKey.Instance().Id != 4)
                return false;
            if (!smallBlobKey.Remove())
                return false;
            if (smallBlobKey.IsPresent())
                return false;
            return true;
        }
    }
    public class SmallBlobKey : IMultitonKey
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=kynsexstorage;AccountKey=OCwrI4pGQtjc+HEfFetZ0TzExKfum2PrUfcao6cjQEyTfw1mJ15b2vNMWoBGYRkHsXwXJ/WqZXyy6BONehar+Q==;EndpointSuffix=core.windows.net";
        static SmallBlobKey()
        {
            MultitonInstaller.Configure<SmallBlobKey, SmallBlob>(ConnectionString, InCloudType.BlobStorage, CacheExpireTime.EightHour, MultitonExpireTime.TurnOff);
        }
    }
    public class SmallBlob : IMultiton
    {
        public int Id { get; set; }
        public IMultiton Fetch(IMultitonKey key)
        {
            SmallBlobKey smallBlobKey = (SmallBlobKey)key;
            return new SmallBlob()
            {
                Id = smallBlobKey.Id
            };
        }
    }
}
