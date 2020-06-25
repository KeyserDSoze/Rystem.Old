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
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            Service3Key serviceKey = new Service3Key() { Id = 2 };
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            Service3 service = serviceKey.Instance() as Service3;
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            serviceKey.Restore(new Service3() { A = "4", C = 0 });
            metrics.CheckIfNotOkExit((serviceKey.Instance() as Service3).A != "4");
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 1);
            metrics.CheckIfNotOkExit(!serviceKey.Remove());
            metrics.CheckIfNotOkExit(serviceKey.IsPresent());
            metrics.CheckIfNotOkExit(serviceKey.Keys().Count != 0);
            service = serviceKey.Instance() as Service3;
            await Task.Delay(200);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(800);
            metrics.CheckIfNotOkExit(!serviceKey.IsPresent());
            await Task.Delay(4800);
            service = serviceKey.Instance() as Service3;
        }
    }
    public class Service3Key : ICacheKey<Service3>
    {
        public int Id { get; set; }
        public Task<Service3> FetchAsync()
        {
            return Task.FromResult(new Service3()
            {
                A = Alea.GetTimedKey(),
                C = Alea.GetNumber(100)
            });
        }

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithCache(CacheConsistency.Always)
                .WithMemory(new MemoryCacheProperties(ExpireTime.FiveSeconds))
                .And()
                 .WithCloud(KeyManager.Instance.Storage)
                    .WithBlobstorage(new BlobStorageCacheProperties(ExpireTime.Infinite))
                    .Build();
        }
    }
    public class Service3
    {
        public string A { get; set; }
        public int C { get; set; }
    }
}


