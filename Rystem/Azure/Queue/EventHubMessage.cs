using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public class EventHubMessage : AConnectionMessage
    {
        public override async Task<DebugMessage> DebugSendFurther(int delay = 0)
        {
            await Task.Delay(delay);
            return await ((IEventHub)this.Container).DebugSend(this.Attempt + 1, this.Installation, this.Flow, this.Version);
        }

        public override async Task<long> SendFurther(int delay = 0)
        {
            await Task.Delay(delay);
            await ((IEventHub)this.Container).Send(this.Attempt + 1, this.Installation, this.Flow, this.Version);
            return 0;
        }
    }
    public static class ExtensionEventHubMessageMethod
    {
        public static EventHubMessage ToEventHubMessage(this string connectionMessage)
        {
            try
            {
                return JsonConvert.DeserializeObject<EventHubMessage>(connectionMessage, NewtonsoftConst.JsonSettings);
            }
            catch
            {
                return new EventHubMessage()
                {
                    Attempt = 0,
                };
            }
        }
    }
}
