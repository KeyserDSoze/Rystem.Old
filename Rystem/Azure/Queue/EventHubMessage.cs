﻿using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public class EventHubMessage : AConnectionMessage
    {
        public override async Task<long> SendFurther(int delay = 0)
        {
            await ((IEventHub)this.Container).Send(this.Attempt + 1, this.Flow, this.Version);
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
