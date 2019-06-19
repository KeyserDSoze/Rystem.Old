﻿using Rystem.Cache;
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
            MultitonUtility.ClearAllCache(Service.ConnectionString).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
    }
    public class ServiceKey : IMultitonKey
    {
        public int Id { get; set; }
        public string Another { get; set; }
    }
    public class Service : IMultiton
    {
        internal const string ConnectionString = "redistest23.redis.cache.windows.net:6380,password=xLnjyPpuHLRb+Z6rJ9PnsnvWTYS4NodjnPyULUNkoa8=,ssl=True,abortConnect=False";
        static Service()
        {
            MultitonInstaller.Configure<Service>(ConnectionString, typeof(ServiceKey), CacheExpireTime.FiveMinutes, MultitonExpireTime.FiveMinutes);
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


