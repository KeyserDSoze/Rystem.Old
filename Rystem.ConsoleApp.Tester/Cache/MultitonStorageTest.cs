using Rystem.Cache;
using Wonda.Engine.Library.Multiton;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Cache
{
    public class MultitonStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            LastBillingOk lastBillingOk = new LastBillingOkKey() { CustomerId = "222", ServiceId = "9"}.Instance();
            new LastBillingOkKey() { CustomerId = "222", ServiceId = "9" }.Restore(new LastBillingOk() { LastEventTime = DateTime.UtcNow.Ticks });
            new LastBillingOkKey() { CustomerId = "222", ServiceId = "9" }.Remove();
            return true;
        }
    }
    
}
namespace Wonda.Engine.Library.Multiton
{
    public class LastBillingOkKey : IMultitonKey
    {
        public string ServiceId { get; set; }
        public string CustomerId { get; set; }
        static LastBillingOkKey()
        {
            MultitonInstaller.Configure<LastBillingOkKey, LastBillingOk>(
                "DefaultEndpointsProtocol=https;AccountName=kynsexstorage;AccountKey=OCwrI4pGQtjc+HEfFetZ0TzExKfum2PrUfcao6cjQEyTfw1mJ15b2vNMWoBGYRkHsXwXJ/WqZXyy6BONehar+Q==;EndpointSuffix=core.windows.net",
                InCloudType.TableStorage,
                 CacheExpireTime.Infinite, MultitonExpireTime.TurnOff);
        }
    }
    public class LastBillingOk : IMultiton
    {
        public long LastEventTime { get; set; }
        public IMultiton Fetch(IMultitonKey key)
        {
            LastBillingOkKey lastBillingOkKey = (LastBillingOkKey)key;
            LastBillingOk lastBillingOk = new LastBillingOk()
            {
                LastEventTime = DateTime.UtcNow.Ticks
            };
            lastBillingOkKey.Restore(lastBillingOk);
            return lastBillingOk;
        }
    }
}
