using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class Toast
    {
        [JsonProperty("delay")]
        public int Delay { get; set; }
        [JsonProperty("animation")]
        public bool Animation { get; set; }
        [JsonProperty("autohide")]
        public bool Autohide { get; set; }
        [JsonProperty("cssClass")]
        public bool CssClass { get; set; }
        public static Toast Default = new Toast() { Animation = true, Autohide = true, Delay = 1000 };
    }
}
