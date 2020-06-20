using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Rystem.DistributedLock;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.DistributedLock
{
    public class BlobLockTest : IUnitTest
    {
        private const string LeaseId = "17051432-2c32-4029-bd17-847fb491d90a";
        private const string LeaseId2 = "17051442-2c32-4029-bd17-847fb491d90a";
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            //var client = new BlobServiceClient(TableStorageTester.ConnectionString);
            //var container = client.GetBlobContainerClient("calcutta");
            //var context = container.GetBlobClient($"Lock_soros");
            //if (!container.Exists())
            //    container.CreateIfNotExists();
            //if (!context.Exists())
            //    context.Upload(new MemoryStream(new byte[0]));
            //var lease = container.GetBlobLeaseClient(LeaseId);
            //var lease3 = container.GetBlobLeaseClient(LeaseId2);
            //Response<BlobLease> response = lease.AcquireAsync(new TimeSpan(-1)).ToResult();
            //try
            //{
            //    Response<BlobLease> response2 = lease3.AcquireAsync(new TimeSpan(-1)).ToResult();
            //}
            //catch (Exception er)
            //{
            //    string ora = er.ToString();
            //}
            //var lease2 = container.GetBlobLeaseClient(LeaseId);
            //try
            //{
            //    await lease2.ReleaseAsync().NoContext();
            //}
            //catch
            //{
            //    await lease.ReleaseAsync().NoContext();
            //}
            new Duby().Acquire();
            new Duby().Acquire();
            new Duby().Acquire();
            new Duby().Release();
            new Duby().Release();
            new Duby().Acquire();
            new Duby().Release();
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
