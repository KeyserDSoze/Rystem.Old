using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Debug;
using Rystem.Enums;
using System;
using System.Threading.Tasks;

namespace Rystem.Azure.Queue
{
    public abstract class AConnectionMessage
    {
        [JsonProperty("A")]
        public int Attempt { get; set; }
        [JsonProperty("C")]
        public dynamic Container { get; set; }
        [JsonProperty("F")]
        public FlowType Flow { get; set; } = FlowType.Flow0;
        [JsonProperty("V")]
        public VersionType Version { get; set; } = VersionType.V0;
        [JsonProperty("I")]
        public Installation Installation { get; set; } = Installation.Default;
        private DateTime? eventTimeStamp;
        [JsonProperty("T")]
        public DateTime EventTimeStamp { get { return (DateTime)(eventTimeStamp ?? (eventTimeStamp = DateTime.UtcNow)); } set { eventTimeStamp = value; } }
        [JsonProperty("G")]
        public DateTime AggregatedTimeStamp => new DateTime(this.EventTimeStamp.Year, this.EventTimeStamp.Month, this.EventTimeStamp.Day, this.EventTimeStamp.Hour, (this.EventTimeStamp.Minute / 5) * 5, 0);
        [JsonProperty("B")]
        public int IsBatch { get; set; } = 0;
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, NewtonsoftConst.JsonSettings);
        }
        public T ToObject<T>()
        {
            return (T)this.Container;
        }
        public abstract long SendFurther(int delay = 0);
        public abstract DebugMessage DebugSendFurther(int delay = 0);
        public abstract Task<long> SendFurtherAsync(int delay = 0);
        public abstract Task<DebugMessage> DebugSendFurtherAsync(int delay = 0);
    }
}
