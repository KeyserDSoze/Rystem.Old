using Rystem.Cache;
using Rystem.ConsoleApp.Tester;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonBlobStorageTest : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            SmallBlobKey smallBlobKey = new SmallBlobKey() { Id = 2 };
            smallBlobKey.Remove();
            if (smallBlobKey.IsPresent())
                return false;
            SmallBlob smallBlob = smallBlobKey.Instance() as SmallBlob;
            if (!smallBlobKey.IsPresent())
                return false;
            smallBlobKey.Restore(new SmallBlob() { Id = 4 });
            if ((smallBlobKey.Instance() as SmallBlob).Id != 4)
                return false;
            if (smallBlobKey.AllKeys().Count != 1)
                return false;
            if (!smallBlobKey.Remove())
                return false;
            if (smallBlobKey.IsPresent())
                return false;
            if (smallBlobKey.AllKeys().Count != 0)
                return false;
            smallBlob = smallBlobKey.Instance() as SmallBlob;
            Thread.Sleep(200);
            if (!smallBlobKey.IsPresent())
                return false;
            Thread.Sleep(4800);
            if (smallBlobKey.IsPresent())
                return false;
            return true;
        }
    }
    public class SmallBlobKey : IMultitonKey
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static SmallBlobKey()
        {
            MultitonInstaller.Configure<SmallBlobKey, SmallBlob>(
                new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.BlobStorage, ExpireTime.FiveSeconds)));
        }
        public IMultiton Fetch()
        {
            return new SmallBlob()
            {
                Id = this.Id
            };
        }
    }
    public class SmallBlob : IMultiton
    {
        public int Id { get; set; }
    }
}
