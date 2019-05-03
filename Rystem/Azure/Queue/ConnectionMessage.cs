using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class ConnectionMessage
    {
        [JsonProperty("A")]
        public int Attempt { get; set; }
        [JsonProperty("C")]
        public dynamic Container { get; set; }
        [JsonProperty("F")]
        public FlowType Flow { get; set; } = FlowType.Flow0;
        [JsonProperty("V")]
        public VersionType Version { get; set; } = VersionType.V0;
        private DateTime? eventTimeStamp;
        [JsonProperty("T")]
        public DateTime EventTimeStamp { get { return (DateTime)(eventTimeStamp ?? (eventTimeStamp = DateTime.UtcNow)); } set { eventTimeStamp = value; } }
        [JsonProperty("G")]
        public DateTime AggregatedTimeStamp => new DateTime(this.EventTimeStamp.Year, this.EventTimeStamp.Month, this.EventTimeStamp.Day, this.EventTimeStamp.Hour, (this.EventTimeStamp.Minute / 5) * 5, 0);
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, NewtonsoftConst.JsonSettings);
        }
        public T ToObject<T>() where T : new()
        {
            return this.Container.ToObject<T>();
        }
    }
    public static class ExtensionConnectionMessageMethod
    {
        public static ConnectionMessage ToConnectionMessage(this string connectionMessage)
        {
            try
            {
                return JsonConvert.DeserializeObject<ConnectionMessage>(connectionMessage);
            }
            catch
            {
                return new ConnectionMessage()
                {
                    Attempt = 0,
                };
            }
        }
    }
}
