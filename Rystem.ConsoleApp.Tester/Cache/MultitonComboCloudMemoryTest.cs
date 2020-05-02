using Rystem.Cache;
using Rystem.UnitTest;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonComboCloudMemoryTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Service3Key serviceKey = new Service3Key() { Id = 2 };
            if (serviceKey.IsPresent())
                return false;
            Service3 service = serviceKey.Instance() as Service3;
            if (!serviceKey.IsPresent())
                return false;
            serviceKey.Restore(new Service3() { A = "4", C = 0 });
            if ((serviceKey.Instance() as Service3).A != "4")
                return false;
            if (serviceKey.Keys().Count != 1)
                return false;
            if (!serviceKey.Remove())
                return false;
            if (serviceKey.IsPresent())
                return false;
            if (serviceKey.Keys().Count != 0)
                return false;
            service = serviceKey.Instance() as Service3;
            Thread.Sleep(200);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(800);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(4800);
            service = serviceKey.Instance() as Service3;
            return true;
        }
    }
    public class Service3Key : IMultitonKey<Service3>
    {
        public int Id { get; set; }
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public Service3 Fetch()
        {
            return new Service3()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            };
        }
        static Service3Key()
            => MultitonInstaller.Configure<Service3Key, Service3>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.BlobStorage, ExpireTime.Infinite), new ExpiringProperties(ExpireTime.FiveSeconds)));
    }
    public class Service3 : IMultiton
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


