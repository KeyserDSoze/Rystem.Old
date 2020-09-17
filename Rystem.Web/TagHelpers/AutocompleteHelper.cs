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
    [HtmlTargetElement("rystem-autocomplete", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AutocompleteHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-retrieve-context")]
        public RequestContext RetrieveContext { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-min-length")]
        public int MinimumLength { get; set; } = 3;
        private string Id { get; } = $"rystem-autocomplete-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string AutcompleteScript = "<script>$(document).ready(function() {{new AutocompleteRystem('{0}', {1}, {2}, {3}).show();}});</script>";
        private const string EmptyFunction = "undefined";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            while (output.Attributes.Count > 0)
                output.Attributes.RemoveAt(0);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<input type=\"text\" id=\"{this.Id}\"");
            foreach (var attribute in context.AllAttributes)
                stringBuilder.Append($" {attribute.Name}='{attribute.Value}' ");
            stringBuilder.Append($"/><div id=\"suggesstion-box-{this.Id}\"></div>");
            output.Content.AppendHtml(stringBuilder.ToString());
            output.PostContent.AppendHtml(string.Format(AutcompleteScript,
                this.Id,
                this.RequestContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction,
                this.RetrieveContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction,
                this.MinimumLength
                ));
            return Task.CompletedTask;
        }
    }
}
