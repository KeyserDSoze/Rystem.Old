using Rystem.Cache;
using Rystem.Interfaces.Utility.Tester;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class MultitonTest : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
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
            if (serviceKey.AllKeys().Count != 1)
                return false;
            if (!serviceKey.Remove())
                return false;
            if (serviceKey.IsPresent())
                return false;
            if (serviceKey.AllKeys().Count != 0)
                return false;
            MultitonUtility.ClearAllCacheAsync(ServiceKey.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class ServiceKey : IMultitonKey
    {
        public int Id { get; set; }
        public IMultiton Fetch()
        {
            return new Service()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            };
        }
        internal const string ConnectionString = "testredis23.redis.cache.windows.net:6380,password=6BSgF1XCFWDSmrlvm8Kn3whMZ3s2pOUH+TyUYfzarNk=,ssl=True,abortConnect=False";
        static ServiceKey()
        {
            MultitonInstaller.Configure<ServiceKey>(ConnectionString, InCloudType.RedisCache, CacheExpireTime.TenMinutes, MultitonExpireTime.TurnOff);
        }
    }
    public class Service : IMultiton
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


