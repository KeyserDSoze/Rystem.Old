using Rystem.DistributedLock;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.DistributedLock
{
    public class LockTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await new Duby().AcquireAsync();
            await new Duby().AcquireAsync();
            await new Duby().AcquireAsync();
            await new Duby().ReleaseAsync();
            await new Duby().ReleaseAsync();
            await new Duby().AcquireAsync();
            await new Duby().ReleaseAsync();
            return true;
        }
        private class Duby : ILock
        {
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithLock(TableStorageTester.ConnectionString)
                    .WithBlobStorage(new BlobBuilder())
                    .Build();
            }
        }
    }
}
