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
    [HtmlTargetElement("rystem-ajax-button", Attributes = "rystem-request-context", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AjaxButtonHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-update-context")]
        public RequestContext UpdateRequestContext { get; set; }
        [HtmlAttributeName("rystem-modal-close")]
        public bool CloseModal { get; set; }
        [HtmlAttributeName("rystem-toast")]
        public Toast Toast { get; set; }

        private string Id { get; } = $"rystem-ajax-button-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string RystemAsyncFormShow = "new AjaxButtonRystem('{0}', {1}, {2}, {3}, {4}).submit(event, this);";
        private const string EmptyFunction = "undefined";

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "button";
            output.Attributes.Add("onclick",
                string.Format(RystemAsyncFormShow,
                    this.Id,
                    (this.RequestContext ?? new RequestContext() { RequestType = RequestType.Post }).FinalizeRequestContext(this.ViewContext.HttpContext.Request),
                    this.CloseModal.ToString().ToLower(),
                    this.Toast?.ToJsonNoNull() ?? EmptyFunction,
                    this.UpdateRequestContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction
                    ));
            return Task.CompletedTask;
        }
    }
}
