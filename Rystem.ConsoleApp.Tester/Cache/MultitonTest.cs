using Rystem.Cache;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Cache
{
    public class MultitonTest : ITest
    {
        public bool DoWork(string entry)
        {
            for (int i = 0; i < 2; i++)
            {
                ServiceKey serviceKey = new ServiceKey()
                {
                    Id = i,
                    Another = "ssss"
                };
                List<ServiceKey> serviceKeys = serviceKey.List();
                Console.WriteLine($"List must be zero: {serviceKeys.Count}");
                if (serviceKeys.Count > 0) return false;
                bool updating = serviceKey.Update();
                if (!updating) return false;
                updating = serviceKey.Update(new Service()
                {
                    A = "updating",
                    C = 20
                });
                if (!updating) return false;
                Service service = serviceKey.Instance();
                if (service == null || service.A != "updating") return false;
                bool exists = serviceKey.Exists();
                if (!exists) return false;
                serviceKeys = serviceKey.List();
                Console.WriteLine($"List must be one: {serviceKeys.Count}");
                if (serviceKeys.Count != 1) return false;
                bool deleting = serviceKey.Delete();
                if (!deleting) return false;
            }
            MultitonUtility.ClearAllCache(Service.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class ServiceKey : IMultitonKey
    {
        public int Id { get; set; }
        public string Another { get; set; }
        public Type MultitonType => typeof(Service);
    }
    public class Service : IMultiton
    {
        internal const string ConnectionString = "redistest23.redis.cache.windows.net:6380,password=xLnjyPpuHLRb+Z6rJ9PnsnvWTYS4NodjnPyULUNkoa8=,ssl=True,abortConnect=False";
        static Service()
        {
            MultitonInstall<Service>.OnStart(ConnectionString, CacheExpireTime.FiveMinutes, MultitonExpireTime.FiveMinutes);
        }
        public string A { get; set; }
        public int C { get; set; }
        public IMultiton Fetch(IMultitonKey key)
        {
            return new Service()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            };
        }
    }
}


