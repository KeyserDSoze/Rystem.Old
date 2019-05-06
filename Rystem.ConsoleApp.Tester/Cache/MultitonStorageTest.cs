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
            new LastBillingOkKey() { CustomerId = "222", ServiceId = "9" }.Update(new LastBillingOk() { LastEventTime = DateTime.UtcNow.Ticks });
            new LastBillingOkKey() { CustomerId = "222", ServiceId = "9" }.Delete();
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
        public Type MultitonType => typeof(LastBillingOk);
    }
    public class LastBillingOk : IMultiton
    {
        public long LastEventTime { get; set; }
        static LastBillingOk()
        {
            MultitonInstall<LastBillingOk>.OnStart("DefaultEndpointsProtocol=https;AccountName=wondacustomerbase;AccountKey=+pSYUSkVNAn1t2CJXqd38o4XtN6bmBbEqtDxz7KS6kIxM6ZWD6fSckKEVFiLRDCG2y6BjoPRVOvjU0kDbV3WnA==;EndpointSuffix=core.windows.net", CacheExpireTime.Infinite, MultitonExpireTime.TurnOff);
        }

        public IMultiton Fetch(IMultitonKey key)
        {
            LastBillingOkKey lastBillingOkKey = (LastBillingOkKey)key;
            LastBillingOk lastBillingOk = new LastBillingOk()
            {
                LastEventTime = DateTime.UtcNow.Ticks
            };
            lastBillingOkKey.Update(lastBillingOk);
            return lastBillingOk;
        }
    }
}
