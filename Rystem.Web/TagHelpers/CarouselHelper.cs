using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-carousel", Attributes = "rystem-data, rystem-options", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class CarouselHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-data")]
        public IEnumerable<ICarouselItem> Data { get; set; }
        [HtmlAttributeName("rystem-options")]
        public CarouselComponent Options { get; set; }
        private string Id { get; } = $"rystem-carousel-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string SwiperScript = "<script>new CarouselRystem('{0}', '{1}', {2}, {3}).show();</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string containerId = $"container-{this.Id}";
            output.TagName = "div";
            output.Content.AppendHtml($"<div id='{containerId}'></div>");
            output.PostContent.AppendHtml(string.Format(SwiperScript,
                this.Id,
                containerId,
                this.Data.Select(x => new CarouselItem() { Content = x.Content, Label = x.Label, Link = x.Link }).ToDefaultJson(),
                (this.Options ?? CarouselComponent.Default).ToJsonNoNull()));
            return Task.CompletedTask;
        }
    }
}
