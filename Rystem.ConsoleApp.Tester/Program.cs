using Rystem.Cache;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Wonda.Engine.Library.Multiton;

namespace Rystem.ConsoleApp.Tester
{
    class Program
    {
        static void Main()
        {
            //IUnitTestMachine testMachine = new ConsoleMachine<Program>("Rystem.ZConsoleApp.Tester");
            //testMachine.Start();
            new CustomerBaseKey() { ServiceId = 707, CustomerId = "393924460101" }.Instance();
        }
    }
}
namespace Wonda.Engine.Library.Multiton
{
    public sealed class CustomerBaseKey : IMultitonKey<CustomerBase>
    {
        private const string CustomerBase = "wondacustomerbase.redis.cache.windows.net:6380,password=33YSMRi0mgAiJX2ZihqayQWhQCut+QSus1317kXG2CI=,ssl=True,abortConnect=False";
        public string CustomerId { get; set; }
        public int ServiceId { get; set; }
        [NoMultitonKey]
        public int CarrierId { get; set; }
        [NoMultitonKey]
        public string PublisherId { get; set; }
        [NoMultitonKey]
        public string PublisherTransactionId { get; set; }
        [NoMultitonKey]
        public int FlowId { get; set; }
        static CustomerBaseKey()
        {
            MultitonInstaller.Configure<CustomerBaseKey, CustomerBase>(new MultitonProperties(new InCloudMultitonProperties(CustomerBase, InCloudType.RedisCache, ExpireTime.SixMonths, 10)));
        }
        public CustomerBase Fetch()
        {
            return default;
        }
    }
    public sealed class CustomerBase : IMultiton
    {
        public int ID
        {
            get;
            set;
        }
        public string CustomerId { get; set; }
        public int ServiceId { get; set; }
        public bool? IsActive { get; set; }
        public int Carrier { get; set; }
        public int Country { get; set; }
        public long CreationDate { get; set; }
        public long FirstSubscriptionDate { get; set; }
        public int FlowId { get; set; }
        /// <summary>
        /// Ultimo valore temporale di passaggio per uno AStartState
        /// </summary>
        public long LastStartState { get; set; } = 0;
        /// <summary>
        /// Ultimo id della BillingStateMachine in attesa
        /// </summary>
        public long LastBillingState { get; set; } = 0;
        public decimal SubscriptionBilled { get; set; } = 0;
        public string ExternalAuthId { get; set; }
        public string PublisherId { get; set; }
        public string PublisherTransactionId { get; set; }

    }
}
