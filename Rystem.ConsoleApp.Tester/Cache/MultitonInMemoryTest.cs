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
    public class MultitonInMemoryTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Service2Key serviceKey = new Service2Key() { Id = 2 };
            if (serviceKey.IsPresent())
                return false;
            Service2 service = serviceKey.Instance() as Service2;
            if (!serviceKey.IsPresent())
                return false;
            serviceKey.Restore(new Service2() { A = "4", C = 0 });
            if ((serviceKey.Instance() as Service2).A != "4")
                return false;
            if (serviceKey.Keys().Count != 1)
                return false;
            if (!serviceKey.Remove())
                return false;
            if (serviceKey.IsPresent())
                return false;
            if (serviceKey.Keys().Count != 0)
                return false;
            service = serviceKey.Instance() as Service2;
            Thread.Sleep(200);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(800);
            if (serviceKey.IsPresent())
                return false;
            serviceKey.Restore(expiringTime : new TimeSpan(0, 0, 5));
            Thread.Sleep(2000);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(5000);
            if (serviceKey.IsPresent())
                return false;
            return true;
        }
    }
    public class Service2Key : IMultitonKey<Service2>
    {
        public int Id { get; set; }
        public Service2 Fetch()
        {
            return new Service2()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            };
        }
        static Service2Key()
        {
            MultitonInstaller.Configure<Service2Key, Service2>(new MultitonProperties(new ExpiringProperties(ExpireTime.OneSecond)));
        }
    }
    public class Service2 : IMultiton
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


