using Rystem.Cache;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonBlobStorageTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            SmallBlobKey smallBlobKey = new SmallBlobKey() { Id = 2 };
            smallBlobKey.Remove();
            if (smallBlobKey.IsPresent())
                return false;
            SmallBlob smallBlob = smallBlobKey.Instance();
            if (!smallBlobKey.IsPresent())
                return false;
            smallBlobKey.Restore(new SmallBlob() { Id = 4 });
            if ((smallBlobKey.Instance() as SmallBlob).Id != 4)
                return false;
            if (smallBlobKey.Keys<SmallBlobKey, SmallBlob>().Count != 1)
                return false;
            if (!smallBlobKey.Remove())
                return false;
            if (smallBlobKey.IsPresent())
                return false;
            if (smallBlobKey.Keys<SmallBlobKey, SmallBlob>().Count != 0)
                return false;
            smallBlob = smallBlobKey.Instance();
            Thread.Sleep(200);
            if (!smallBlobKey.IsPresent())
                return false;
            Thread.Sleep(4800);
            if (smallBlobKey.IsPresent())
                return false;
            smallBlobKey.Restore(expiringTime: new TimeSpan(0, 0, 10));
            Thread.Sleep(7000);
            if (!smallBlobKey.IsPresent())
                return false;
            Thread.Sleep(5000);
            if (smallBlobKey.IsPresent())
                return false;
            return true;
        }
    }
    public class SmallBlobKey : IMultitonKey<SmallBlob>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        static SmallBlobKey()
        {
            MultitonInstaller.Configure<SmallBlobKey, SmallBlob>(
                new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.BlobStorage, ExpireTime.FiveSeconds)));
        }
        public SmallBlob Fetch()
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
