using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public interface ICarouselItem
    {
        [JsonProperty("link")]
        string Link { get; }
        [JsonProperty("label")]
        string Label { get; }
        [JsonProperty("content")]
        string Content { get; }
    }
    public class CarouselItem : ICarouselItem
    {
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
