using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using Wonda.Engine.Library.Models.Enumerator;
using Wonda.Engine.Library.Multiton.Facade;

namespace Rystem.ConsoleApp.Tester.Cache
{
    public class MultitonTest : ITest
    {
        public bool DoWork(string entry)
        {
            ServiceKey serviceKey = new ServiceKey()
            {
                Id = 2459
            };
            MultitonManager<Service>.Update(serviceKey);
            Service service = MultitonManager<Service>.Instance(serviceKey);
            return true;
        }
    }
}
namespace Wonda.Engine.Library.Multiton.Facade
{
    public class ServiceKey : AMultitonKey
    {
        public string ExternalServiceId { get; set; } = string.Empty;
        public int Id { get; set; } = 0;
    }
    public class Service : AMultiton
    {
        static Service()
        {
            MultitonInstall<Service>.OnStart("registry.redis.cache.windows.net:6380,password=jGfirMv9BP0lxQGmzQYPIRbCNVlL9gsYW1XlsfGM6Rc=,ssl=True,abortConnect=False", CacheExpireTime.Infinite, MultitonExpireTime.TenMinutes);
        }
        public int Id { get; set; }
        public string Label { get; set; }
        public string ExternalServiceId { get; set; }
        public int SubjectId { get; set; }
        public int CarrierId { get; set; }
        public int IntegrationId { get; set; }
        public bool HasSubscription { get; set; }
        public string UniquePublisherChannel { get; set; }
        public string Uri { get; set; }
        public List<Flow> Flows { get; set; } = new List<Flow>();
        public Dictionary<MessageType, string> Messages { get; set; } = new Dictionary<MessageType, string>();
        public Dictionary<string, string> OptionalParameters { get; set; } = new Dictionary<string, string>();
        public IntegrationParameterService IntegrationParameterService { get; set; }

        public override AMultiton Fetch(AMultitonKey key)
        {
            ServiceKey serviceKey = (ServiceKey)key;
            List<Flow> flows = new List<Flow>();

            Flow flow = new Flow()
            {
                Id = 1
            };


            Channel channel = new Channel()
            {
                Reserve = 0,
                Retry = 0,
                Suspension = 0,
                Cost = 0,
                ExpiresIn = 0,
                Id = 0,
                Duration = 0
            };


            channel.Fractions.Add((decimal)3);

            flow.Channels.Add(channel);

            flows.Add(flow);

            Dictionary<MessageType, string> messages = new Dictionary<MessageType, string>();
            messages.Add(MessageType.Activation, string.Empty);


            Dictionary<string, string> optionalParameters = new Dictionary<string, string>();
                    optionalParameters.Add(string.Empty, string.Empty);

            IntegrationParameterService integrationParameterService = null;
           
            return new Service()
            {
                Id =serviceKey.Id,
                CarrierId = 0,
                ExternalServiceId = "",
                HasSubscription = true,
                Label = "",
                SubjectId = 0,
                Flows = flows,
                Messages = messages,
                OptionalParameters = optionalParameters,
                Uri = "",
                UniquePublisherChannel = "",
                IntegrationParameterService = integrationParameterService,
                IntegrationId = 0
            };
        }
    }
    public class Flow
    {
        public int Id { get; set; }
        public List<Channel> Channels { get; set; } = new List<Channel>();
    }
    public class Channel
    {
        public int Id { get; set; }
        public decimal Reserve { get; set; }
        public int Retry { get; set; }
        public int Suspension { get; set; }
        public decimal Cost { get; set; }
        public long? ExpiresIn { get; set; }
        public int Duration { get; set; }
        public long DurationTicks => new TimeSpan(this.Duration, 0, 0).Ticks;
        public List<decimal> Fractions { get; set; } = new List<decimal>();
    }
    public class IntegrationParameterService
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int? DelayOk { get; set; }
        public int? DelayNotOk { get; set; }
        public int? DelayNotificationNotOk { get; set; }
        public int? DelayDeactivationNotOk { get; set; }
        public int? DelayEndBillingFlow { get; set; }
        public int? DeactivationNotOk { get; set; }
        public int? NotificationNotOk { get; set; }
        public int? StartBilling { get; set; }
        public int? StartNotification { get; set; }
        public bool? WaitNotification { get; set; }
        public bool? AsyncBilling { get; set; }
        public bool? NotificationIsActive { get; set; }
        public bool? BillingIsActive { get; set; }
        public bool? RenewIsActive { get; set; }
        public bool? DeactivationWithNotification { get; set; }
        public bool? DeactivationNeedsNotification { get; set; }
        public string EnrichmentUrl { get; set; }
        public string EnrichmentUrlOut { get; set; }
        public int? PossibleAttempt { get; set; }
    }
}

namespace Wonda.Engine.Library.Models.Enumerator
{
    public enum MessageType
    {
        Activation,
        Deactivation,
        AlreadyDeactivated,
        ErrorMo,
        Optional,
        MonthlyReminder,
        WeeklyReminder,
        TrueWeeklyReminder,
        RetryBilling,
        Reactivation,
        NoCredit,
        SinglePayment,
        Trial
    }
}

