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
    [HtmlTargetElement("rystem-chart", Attributes = "rystem-data", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ChartHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-data")]
        public DataChart Data { get; set; }
        private string Id { get; } = $"rystem-carousel-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string SwiperScript = "<script>new ChartRystem('{0}', {1}).show();</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Content.AppendHtml($"<canvas id='{this.Id}' width='400' height='400'></canvas>");
            output.PostContent.AppendHtml(string.Format(SwiperScript,
                this.Id,
                this.Data.ToJson()));
            return Task.CompletedTask;
        }
    }
}
