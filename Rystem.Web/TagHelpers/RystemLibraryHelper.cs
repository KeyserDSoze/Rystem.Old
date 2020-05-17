using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem", Attributes = "", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RystemLibraryHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("jquery-cdn")]
        public bool UseJQueryCoreCdn { get; set; } = true;
        [HtmlAttributeName("jquery-ui-cdn")]
        public JQueryUIType UseJQueryUiCdn { get; set; }
        [HtmlAttributeName("bootstrap-cdn")]
        public bool UseBootstrapCdn { get; set; } = true;
        [HtmlAttributeName("rystem-language")]
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "rystem";
            StringBuilder stringBuilder = new StringBuilder();
            if (this.UseJQueryCoreCdn)
                stringBuilder.Append("<script src='https://code.jquery.com/jquery-3.5.1.min.js' integrity='sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=' crossorigin='anonymous'></script>");
            if (this.UseJQueryUiCdn != JQueryUIType.None)
            {
                stringBuilder.Append("<script src='https://code.jquery.com/ui/1.12.1/jquery-ui.min.js' integrity='sha256-VazP97ZCwtekAsvgPBSUwPFKdrwD3unUfSGVYrahUqU=' crossorigin='anonymous'></script>");
                switch (this.UseJQueryUiCdn)
                {
                    case JQueryUIType.Light:
                        stringBuilder.Append("<link rel='stylesheet' href='https://code.jquery.com/ui/1.11.4/themes/ui-lightness/jquery-ui.css' />");
                        break;
                    case JQueryUIType.Dark:
                        stringBuilder.Append("<link rel='stylesheet' href='https://code.jquery.com/ui/1.12.1/themes/ui-darkness/jquery-ui.css' />");
                        break;
                }
            }
            if (this.UseBootstrapCdn)
            {
                stringBuilder.Append("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css' />");
                stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/bootstrap-select.css' />");
                stringBuilder.Append("<script src='https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js' integrity='sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q' crossorigin='anonymous'></script>");
                stringBuilder.Append("<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js'></script>");
                stringBuilder.Append("<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.min.js'></script>");
                stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/bootstrap-select.js'></script>");
                if (Culture != CultureInfo.InvariantCulture)
                    stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/i18n/defaults-{this.Culture.Name}.js'></script>");
            }
            stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.css' /><script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.js'></script>");
            output.Content.SetHtmlContent(stringBuilder.ToString());
            return Task.CompletedTask;
        }
    }
    public enum JQueryUIType
    {
        None,
        Light,
        Dark
    }
}
