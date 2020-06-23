using Rystem.Fast;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rystem.Cache;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class FastCacheTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            var sv = new SaveTheLastDance() { Hola = "DD", Id = 4 };
            string key = "MyKey";
            await sv.SetCacheAsync(key).NoContext();
            var tv = await key.FromCacheAsync<SaveTheLastDance>().NoContext();
            metrics.CheckIfNotOkExit(sv.Id != tv.Id);
            metrics.CheckIfNotOkExit(!await key.ExistCacheAsync().NoContext());
            metrics.CheckIfNotOkExit(!await key.RemoveCacheAsync().NoContext());
            metrics.CheckIfNotOkExit(await key.ExistCacheAsync().NoContext());
            tv = await key.FromCacheAsync<SaveTheLastDance>().NoContext();
            metrics.CheckIfNotOkExit(tv != null);
        }
        private class SaveTheLastDance
        {
            public int Id { get; set; }
            public string Hola { get; set; }
        }
    }
}
