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
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-date", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class DatePickerHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-date-options")]
        public DatePicker Data { get; set; }
        [HtmlAttributeName("name")]
        public string Name { get; set; }
        [HtmlAttributeName("value")]
        public DateTime? Value { get; set; }
        private string Id { get; } = $"rystem-datepicker-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string DatePickerScript = "<script>$(document).ready(function() {{$('#{0}').datepicker({1});}});</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (context.AllAttributes["class"] != null)
                output.Attributes.Remove(context.AllAttributes["class"]);
            output.Content.AppendHtml($"<input id='{this.Id}' name='{this.Name}' class='{context.AllAttributes["class"]?.Value} rystem-datepicker' value='{this.Value:yyyy/MM/dd}' readonly/>");
            output.Content.AppendHtml(string.Format(DatePickerScript,
                this.Id,
                (this.Data ?? DatePicker.Default).ToJsonNoNull()));
            return Task.CompletedTask;
        }
    }
}