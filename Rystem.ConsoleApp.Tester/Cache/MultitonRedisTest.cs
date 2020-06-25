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
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            MultitonUtility.ClearAllCacheAsync(KeyManager.Instance.Redis).ConfigureAwait(false).GetAwaiter().GetResult();
            ServiceKey serviceKey = new ServiceKey() { Id = 2 };
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            Service service = serviceKey.Instance() as Service;
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            serviceKey.Restore(new Service() { A = "4", C = 0 });
            metrics.CheckIfNotOkExit((serviceKey.Instance() as Service).A != "4");
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 1);
            metrics.CheckIfNotOkExit(!serviceKey.Remove());
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 0);
            service = serviceKey.Instance() as Service;
            await Task.Delay(200);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(4800);
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            serviceKey.Restore(expiringTime: new TimeSpan(0, 0, 5));
            await Task.Delay(2000);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(5000);
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            MultitonUtility.ClearAllCacheAsync(KeyManager.Instance.Redis).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
    public class ServiceKey : ICacheKey<Service>
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

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithCache(CacheConsistency.Always)
                .WithCloud(KeyManager.Instance.Redis)
                    .WithRedis(new RedisCacheProperties(ExpireTime.FiveSeconds)).Build();
        }
    }
    public class Service
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


