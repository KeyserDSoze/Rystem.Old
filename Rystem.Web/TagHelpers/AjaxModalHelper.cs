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
    [HtmlTargetElement("rystem-ajax-modal", Attributes = "rystem-request-context,rystem-size", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AjaxModalHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-update-context")]
        public RequestContext UpdateRequestContext { get; set; }
        [HtmlAttributeName("rystem-size")]
        public SizeType Size { get; set; }

        private string Id { get; } = $"rystem-ajax-modal-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string RystemAsyncModalShow = "new ModalRystem('{0}', {1}).show(event, this, {2}, '{3}');";
        private const string EmptyFunction = "undefined";
#warning Creare function per popup di errore o di ok, forse basta solo l'errore, comunque creare il popup

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add(new TagHelperAttribute("onclick",
                string.Format(RystemAsyncModalShow,
                    this.Id,
                    this.UpdateRequestContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction,
                    this.RequestContext.FinalizeRequestContext(this.ViewContext.HttpContext.Request),
                    GetClassNameFromSize()
                    ),
                HtmlAttributeValueStyle.DoubleQuotes));
            return Task.CompletedTask;
        }
        private string GetClassNameFromSize()
            => this.Size switch
            {
                SizeType.Small => "modal-sm",
                SizeType.Large => "modal-lg",
                _ => string.Empty,
            };
    }
}
