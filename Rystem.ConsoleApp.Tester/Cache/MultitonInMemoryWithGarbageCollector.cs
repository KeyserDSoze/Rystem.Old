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
    public class MultitonInMemoryWithGarbageCollector : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).NoContext();

            for (int i = 0; i < 3000; i++)
            {
                Service2Key serviceKey = new Service2Key() { Id = i };
                serviceKey.Restore(new Service2() { A = "4", C = 0 });
                await Task.Delay(2).NoContext();
            }
            await Task.Delay(20000);
            int count = (await new Service2Key().KeysAsync().NoContext()).Count;
            if (count > 0)
                return false;
            return true;
        }
        private class Service2Key : IMultitonKey<Service2>
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
            static Service2Key()
            {
                MultitonInstaller.Configure<Service2Key, Service2>(new MultitonProperties(new ExpiringProperties(ExpireTime.TenSeconds, true, true), CacheConsistency.Always));
            }
        }
        private class Service2 : IMultiton
        {
            public string A { get; set; }
            public int C { get; set; }
        }
    }
}


