using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester
{
    internal class KeyValue : IKeyValue
    {
        public string Storage => "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";

        public string Sql => "Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public string EventHub => "Endpoint=sb://testone2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KD7fVSnPLrPp6E+Q3iDDfiuCf1pgz9MjKHK805/Hdqw=";

        public string ServiceBus => "Endpoint=sb://testone3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GbBogIG4NIPjyzb5qdr0VCH3fFmGSxXt9xChxtfkdVw=";
        public string Redis => "testredis23.redis.cache.windows.net:6380,password=6BSgF1XCFWDSmrlvm8Kn3whMZ3s2pOUH+TyUYfzarNk=,ssl=True,abortConnect=False";
        public static KeyValue Instance { get; } = new KeyValue();
        public KeyValue() { }
    }
}
