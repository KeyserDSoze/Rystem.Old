using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class DropdownItem
    {
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("selected")]
        public bool Selected { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}
