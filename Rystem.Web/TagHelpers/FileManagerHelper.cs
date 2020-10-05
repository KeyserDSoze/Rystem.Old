using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Rystem.Web.Models;
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
    [HtmlTargetElement("rystem-file-manager", Attributes = "rystem-files", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class FileManagerHelper : TagHelper
    {
        private readonly IHtmlHelper HtmlHelper;
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-files")]
        public IEnumerable<FileModel> Files { get; set; }
        [HtmlAttributeName("rystem-localizer")]
        public IStringLocalizer Localizer { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-delete-request-context")]
        public RequestContext DeleteRequestContext { get; set; }
        [HtmlAttributeName("rystem-skip-path")]
        public int Skip { get; set; }
        private string Id { get; } = $"rystem-files-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        public FileManagerHelper(IHtmlHelper htmlHelper)
        {
            HtmlHelper = htmlHelper;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add("id", this.Id);
            (HtmlHelper as IViewContextAware).Contextualize(ViewContext);
            output.Content.SetHtmlContent(await HtmlHelper.PartialAsync("_FileManager", (this.Files, this.Localizer, this.RequestContext, this.DeleteRequestContext, this.Skip)));
        }
    }
}
