using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class DatePicker
    {
        public long? minDate => this.MinDate?.Ticks;
        [JsonIgnore]
        public DateTime? MinDate { get; set; }
        public long? maxDate => this.MaxDate?.Ticks;
        [JsonIgnore]
        public DateTime? MaxDate { get; set; }
        public int firstDay => (int)FirstDay;
        [JsonIgnore]
        public DayOfWeek FirstDay { get; set; } = DayOfWeek.Monday;
        [JsonProperty("showWeek")]
        public bool ShowWeek { get; set; }
        [JsonProperty("autoSize")]
        public bool AutoSize { get; set; }
        public static readonly DatePicker Default = new DatePicker()
        {
            AutoSize = true
        };
    }
    public enum DayOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }
}
