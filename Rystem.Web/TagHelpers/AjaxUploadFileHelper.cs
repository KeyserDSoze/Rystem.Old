using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
    [HtmlTargetElement("rystem-ajax-file", Attributes = "", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AjaxUploadFileHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("name")]
        public string Name { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RoutingContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-options")]
        public DropZone Options { get; set; }
        [HtmlAttributeName("rystem-message")]
        public string Message { get; set; }

        private string Id { get; } = $"rystem-ajax-file-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string RystemDropDownZoneScript = "<script>$(document).ready(function() {{$('#{0}').addClass('dropzone');new Dropzone('#{0}',{1});}});</script>";
        private const string RystemDropDownMessage = "<div class='dz-message d-flex flex-column'><button type='button' class='dz-button'><i class='fas fa-upload'></i></button>{0}</div>";
        private const string DefaultMessage = "Drag & Drop Here or Click";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (this.Options == null)
                this.Options = DropZone.Default;
            if (this.Name != null)
                output.Content.AppendHtml($"<input type='file' id='{this.Id}' name='{this.Name}' {(this.Options.UploadMultiple ? "multiple" : string.Empty)}/>");
            else
                output.Content.AppendHtml($"<div id='{this.Id}' class='dz-clickable'>{string.Format(RystemDropDownMessage, this.Message ?? DefaultMessage)}</div>");
            if (RequestContext != null)
                this.Options.Url = RequestContext.GetUrl(this.ViewContext);
            var options = this.Options.ToJsonNoNull();
            if (this.Options.OnSuccess != null)
                options = options.Replace($"\"{this.Options.OnSuccess}\"", this.Options.OnSuccess);
            output.PostContent.AppendHtml(string.Format(RystemDropDownZoneScript, this.Id, options));
            return Task.CompletedTask;
        }
    }
}