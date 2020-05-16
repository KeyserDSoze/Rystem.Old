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
    [HtmlTargetElement("rystem-ajax-modal", Attributes = "rystem-request-context", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AjaxModalHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-update-context")]
        public RequestContext UpdateRequestContext { get; set; }

        private string Id { get; } = $"rystem-ajax-modal-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string RystemAsyncModalShow = "new Modal('{0}', {1}).show(event, this, {2});";
        private const string EmptyFunction = "null";

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add(new TagHelperAttribute("onclick",
                string.Format(RystemAsyncModalShow,
                    this.Id,
                    this.UpdateRequestContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction,
                    this.RequestContext.FinalizeRequestContext(this.ViewContext.HttpContext.Request)
                    ),
                HtmlAttributeValueStyle.DoubleQuotes));
            return Task.CompletedTask;
        }
    }
}
