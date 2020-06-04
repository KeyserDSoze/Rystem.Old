using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Rystem.Web.Backoffice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Rystem.Web.TagHelpers
{
    [HtmlTargetElement("rystem-navigation", Attributes = "rystem-data", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class NavigationHelper : TagHelper
    {
        private readonly IHtmlHelper HtmlHelper;
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-data")]
        public NavigationBuilder Data { get; set; }
        [HtmlAttributeName("rystem-user")]
        public Func<string, bool> UserIsInRole { get; set; }
        [HtmlAttributeName("rystem-localizer")]
        public IStringLocalizer Localizer { get; set; }
        public NavigationHelper(IHtmlHelper htmlHelper)
        {
            HtmlHelper = htmlHelper;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            (HtmlHelper as IViewContextAware).Contextualize(ViewContext);
            output.Content.SetHtmlContent(await HtmlHelper.PartialAsync("_DefaultNavigation", this.Data.Run(this.Localizer, this.UserIsInRole)));
        }
    }
}