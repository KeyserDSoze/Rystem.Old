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
    public class MultitonBlobStorageTest : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            SmallBlobKey smallBlobKey = new SmallBlobKey() { Id = metrics.ThreadId };
            smallBlobKey.Remove();
            metrics.CheckIfNotOkExit(smallBlobKey.IsPresent());
            SmallBlob smallBlob = smallBlobKey.Instance();
            metrics.CheckIfNotOkExit(!smallBlobKey.IsPresent());
            smallBlobKey.Restore(new SmallBlob() { Id = 4 });
            metrics.CheckIfNotOkExit((smallBlobKey.Instance() as SmallBlob).Id != 4);
            metrics.CheckIfNotOkExit(smallBlobKey.Keys().Count != 1);
            metrics.CheckIfNotOkExit(!smallBlobKey.Remove());
            metrics.CheckIfNotOkExit(smallBlobKey.IsPresent());
            metrics.CheckIfNotOkExit(smallBlobKey.Keys().Count != 0);

            smallBlob = smallBlobKey.Instance();
            await Task.Delay(200);
            metrics.CheckIfNotOkExit(!smallBlobKey.IsPresent());
            await Task.Delay(4800);
            metrics.CheckIfNotOkExit(smallBlobKey.IsPresent());
            smallBlobKey.Restore(expiringTime: new TimeSpan(0, 0, 10));
            await Task.Delay(7000);
            metrics.CheckIfNotOkExit(!smallBlobKey.IsPresent());
            await Task.Delay(5000);
            metrics.CheckIfNotOkExit(smallBlobKey.IsPresent());
        }
    }
    public class SmallBlobKey : ICacheKey<SmallBlob>
    {
        public int Id { get; set; }
        public Task<SmallBlob> FetchAsync()
        {
            return Task.FromResult(new SmallBlob()
            {
                Id = 2
            });
        }

        public ConfigurationBuilder GetConfigurationBuilder()
          => new ConfigurationBuilder().WithCache()
                .WithCloud(KeyManager.Instance.Storage)
                    .WithBlobstorage(new BlobStorageCacheProperties(ExpireTime.FiveSeconds))
            .Build();
    }
    public class SmallBlob
    {
        public int Id { get; set; }
    }
}
