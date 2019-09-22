using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public class QueueMessage : AConnectionMessage
    {
        public override DebugMessage DebugSendFurther(int delay = 0)
        {
            throw new NotImplementedException();
        }

        public override async Task<DebugMessage> DebugSendFurtherAsync(int delay = 0)
        {
            await Task.Delay(delay);
            return await ((IEventHub)this.Container).DebugSendAsync(this.Attempt + 1, this.Installation, this.Flow, this.Version);
        }

        public override long SendFurther(int delay = 0)
        {
            Thread.Sleep(delay);
            return ((IEventHub)this.Container).Send(this.Attempt + 1, this.Installation, this.Flow, this.Version) ? 0 : -1;
        }

        public override async Task<long> SendFurtherAsync(int delay = 0)
        {
            await Task.Delay(delay);
            return (await ((IEventHub)this.Container).SendAsync(this.Attempt + 1, this.Installation, this.Flow, this.Version)) ? 0 : -1;
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
