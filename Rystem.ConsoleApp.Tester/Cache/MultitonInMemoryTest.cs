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
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            Service2Key serviceKey = new Service2Key() { Id = 2 };
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            Service2 service = serviceKey.Instance() as Service2;
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            serviceKey.Restore(new Service2() { A = "4", C = 0 });
            metrics.CheckIfNotOkExit((serviceKey.Instance() as Service2).A != "4");
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 1);
            metrics.CheckIfNotOkExit(!serviceKey.Remove());
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 0);
            service = serviceKey.Instance() as Service2;
            await Task.Delay(200);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(800);
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            serviceKey.Restore(expiringTime: new TimeSpan(0, 0, 5));
            await Task.Delay(2000);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(5000);
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
        }
    }
    public class Service2Key : ICacheKey<Service2>
    {
        public int Id { get; set; }
        public Task<Service2> FetchAsync()
        {
            return Task.FromResult(new Service2()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            });
        }
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .WithCache(CacheConsistency.Always)
                .WithMemory(new MemoryCacheProperties(ExpireTime.OneSecond))
                .Build();
        }
        
    }
    public class Service2 
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


