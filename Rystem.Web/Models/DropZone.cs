using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class DropZone
    {
        public string url => this.Url;
        [JsonIgnore]
        internal string Url { get; set; }
        [JsonProperty("uploadMultiple")]
        public bool UploadMultiple { get; set; }
        [JsonProperty("success", IsReference = true,ItemIsReference =true)]
        public string OnSuccess { get; set; }
        public static DropZone Default => new DropZone();
    }
}