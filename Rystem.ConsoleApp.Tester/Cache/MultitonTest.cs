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
                var x = new ServiceKey() {
                    Id = i
                }.Instance();
                ServiceKey serviceKey = new ServiceKey()
                {
                    Id = i,
                    Another = "ssss"
                };
                serviceKey.ToKeyString();
                List<ServiceKey> serviceKeys = serviceKey.AllKeys();
                Console.WriteLine($"List must be zero: {serviceKeys.Count}");
                if (serviceKeys.Count > 0) return false;
                bool updating = serviceKey.Restore();
                if (!updating) return false;
                updating = serviceKey.Restore(new Service()
                {
                    A = "updating",
                    C = 20
                });
                if (!updating) return false;
                Service service = serviceKey.Instance();
                if (service == null || service.A != "updating") return false;
                bool exists = serviceKey.IsPresent();
                if (!exists) return false;
                serviceKeys = serviceKey.AllKeys();
                Console.WriteLine($"List must be one: {serviceKeys.Count}");
                if (serviceKeys.Count != 1) return false;
                bool deleting = serviceKey.Remove();
                if (!deleting) return false;
            }
            MultitonUtility.ClearAllCache(ServiceKey.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class ServiceKey : IMultitonKey
    {
        public int Id { get; set; }
        public string Another { get; set; }
        internal const string ConnectionString = "testredis23.redis.cache.windows.net:6380,password=6BSgF1XCFWDSmrlvm8Kn3whMZ3s2pOUH+TyUYfzarNk=,ssl=True,abortConnect=False";
        static ServiceKey()
        {
            MultitonInstaller.Configure<Service>(ConnectionString, typeof(ServiceKey), CacheExpireTime.TenMinutes, MultitonExpireTime.FiveMinutes);
        }
    }
    public class Service : IMultiton
    {
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


