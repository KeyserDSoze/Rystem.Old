using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public class ServiceBusMessage : AConnectionMessage
    {
        public override async Task<long> SendFurther(int delay = 0)
        {
            return await ((IServiceBus)this.Container).Send(delay, this.Installation, this.Attempt + 1, this.Flow, this.Version);
        }
    }
    public static class ExtensionServiceBusMessageMessageMethod
    {
        public static ServiceBusMessage ToServiceBusMessage(this string connectionMessage)
        {
            try
            {
                return JsonConvert.DeserializeObject<ServiceBusMessage>(connectionMessage, NewtonsoftConst.JsonSettings);
            }
            catch
            {
                return new ServiceBusMessage()
                {
                    Attempt = 0,
                };
            }
        }
    }
}
