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
    public class MultitonRedisTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).NoContext();
            MultitonUtility.ClearAllCacheAsync(ServiceKey.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            ServiceKey serviceKey = new ServiceKey() { Id = 2 };
            if (serviceKey.IsPresent())
                return false;
            Service service = serviceKey.Instance() as Service;
            if (!serviceKey.IsPresent())
                return false;
            serviceKey.Restore(new Service() { A = "4", C = 0 });
            if ((serviceKey.Instance() as Service).A != "4")
                return false;
            if (serviceKey.Keys().Count != 1)
                return false;
            if (!serviceKey.Remove())
                return false;
            if (serviceKey.IsPresent())
                return false;
            if (serviceKey.Keys().Count != 0)
                return false;
            service = serviceKey.Instance() as Service;
            Thread.Sleep(200);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(4800);
            if (serviceKey.IsPresent())
                return false;
            serviceKey.Restore(expiringTime: new TimeSpan(0, 0, 5));
            Thread.Sleep(2000);
            if (!serviceKey.IsPresent())
                return false;
            Thread.Sleep(5000);
            if (serviceKey.IsPresent())
                return false;
            MultitonUtility.ClearAllCacheAsync(ServiceKey.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class ServiceKey : IMultitonKey<Service>
    {
        public int Id { get; set; }
        public Task<Service> FetchAsync()
        {
            return Task.FromResult(new Service()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            });
        }
        internal const string ConnectionString = "testredis23.redis.cache.windows.net:6380,password=6BSgF1XCFWDSmrlvm8Kn3whMZ3s2pOUH+TyUYfzarNk=,ssl=True,abortConnect=False";
        static ServiceKey()
        {
            MultitonInstaller.Configure<ServiceKey, Service>(new MultitonProperties(new InCloudMultitonProperties(ConnectionString, InCloudType.RedisCache, ExpireTime.FiveSeconds)));
        }
    }
    public class Service : IMultiton
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


