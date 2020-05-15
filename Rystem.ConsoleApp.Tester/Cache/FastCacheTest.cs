using Rystem.Cache;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Cache
{
    public class FastCacheTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            FastCacheInstaller.Configure(new MultitonProperties(new ExpiringProperties(ExpireTime.OneDay, false, false), CacheConsistency.Never));
            var sv = new SaveTheLastDance() { Hola = "DD", Id = 4 };
            string key = "MyKey";
            await sv.SetCacheAsync(key).NoContext();
            var tv = await key.FromCacheAsync<SaveTheLastDance>().NoContext();
            if (sv.Id != tv.Id)
                return false;
            if (!await key.ExistCacheAsync().NoContext())
                return false;
            if (!await key.RemoveCacheAsync().NoContext())
                return false;
            if (await key.ExistCacheAsync().NoContext())
                return false;
            tv = await key.FromCacheAsync<SaveTheLastDance>().NoContext();
            if (tv != null)
                return false;

            return true;
        }
        private class SaveTheLastDance : IMultiton
        {
            public int Id { get; set; }
            public string Hola { get; set; }
        }
    }
}
