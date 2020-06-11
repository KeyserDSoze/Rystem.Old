using Microsoft.AspNetCore.Connections.Features;
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

//spinners: https://freefrontend.com/css-spinners/
namespace Rystem.Web
{
    [HtmlTargetElement("rystem-css", Attributes = "", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RystemCssLibraryHelper : RystemLibraryHelper
    {
        public override bool OnTop => true;
    }
    [HtmlTargetElement("rystem-js", Attributes = "", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RystemJavascriptLibraryHelper : RystemLibraryHelper
    {
        public override bool OnTop => false;
    }
    public abstract class RystemLibraryHelper : TagHelper
    {
        public abstract bool OnTop { get; }
        public bool OnBottom => !OnTop;
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("jquery-cdn")]
        public bool UseJQueryCoreCdn { get; set; } = true;
        [HtmlAttributeName("jquery-ui-cdn")]
        public JQueryUIType UseJQueryUiCdn { get; set; } = JQueryUIType.Light;
        [HtmlAttributeName("bootstrap-cdn")]
        public bool UseBootstrapCdn { get; set; } = true;
        [HtmlAttributeName("swiper-cdn")]
        public bool UseSwiperCdn { get; set; } = true;
        [HtmlAttributeName("font-awesome-cdn")]
        public bool UseFontAwesomeCdn { get; set; } = true;
        [HtmlAttributeName("charjs-cdn")]
        public bool UseCharJsCdn { get; set; } = true;
        [HtmlAttributeName("momentjs-cdn")]
        public bool UseMomentJsCdn { get; set; } = true;
        [HtmlAttributeName("rystem-language")]
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
        [HtmlAttributeName("rystem-default-ui")]
        public DefaultUI DefaultUI { get; set; }
        [HtmlAttributeName("summernote")]
        public bool UseSummernote { get; set; } = true;
        [HtmlAttributeName("rystem-cache")]
        public bool Cache { get; set; } = true;
        [HtmlAttributeName("jquery-datatable")]
        public bool UseDataTable { get; set; } = true;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private readonly static Dictionary<bool, string> JavascriptCssCache = new Dictionary<bool, string>();
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "rystem";
            if (this.Cache && JavascriptCssCache.ContainsKey(this.OnTop))
            {
                output.Content.SetHtmlContent(JavascriptCssCache[this.OnTop]);
                return Task.CompletedTask;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (this.UseJQueryCoreCdn && this.OnTop)
                stringBuilder.Append("<script src='https://code.jquery.com/jquery-3.5.1.min.js' integrity='sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=' crossorigin='anonymous'></script>");
            if (this.UseJQueryUiCdn != JQueryUIType.None)
            {
                if (this.OnBottom)
                    stringBuilder.Append("<script src='https://code.jquery.com/ui/1.12.1/jquery-ui.min.js' integrity='sha256-VazP97ZCwtekAsvgPBSUwPFKdrwD3unUfSGVYrahUqU=' crossorigin='anonymous'></script>");
                if (this.OnTop)
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
                if (this.OnTop)
                {
                    stringBuilder.Append("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css' />");
                    stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/bootstrap-select.css' />");
                }
                if (this.OnBottom)
                {
                    stringBuilder.Append("<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.min.js'></script>");
                    stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/bootstrap-select.js'></script>");
                    if (Culture != CultureInfo.InvariantCulture)
                        stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/i18n/defaults-{this.Culture.Name}.js'></script>");
                }
            }
            if (this.UseSwiperCdn)
            {
                if (this.OnTop)
                    stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/Swiper/5.4.0/css/swiper.min.css' />");
                if (this.OnBottom)
                    stringBuilder.Append("<script src='https://cdnjs.cloudflare.com/ajax/libs/Swiper/5.4.0/js/swiper.min.js'></script>");
            }
            if (this.UseFontAwesomeCdn && this.OnTop)
            {
                stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.13.0/css/all.min.css' />");
            }
            if (this.UseMomentJsCdn)
            {
                stringBuilder.Append("<script src='https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.25.3/moment.min.js'></script>");
            }
            if (this.UseCharJsCdn)
            {
                if (this.OnTop)
                    stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.3/Chart.min.css' />");
                if (this.OnBottom)
                    stringBuilder.Append("<script src='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.3/Chart.min.js'></script>");
            }
            if (this.UseSummernote)
            {
                if (this.OnTop)
                    stringBuilder.Append("<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/summernote@0.8.18/dist/summernote-lite.min.css' />");
                if (this.OnBottom)
                    stringBuilder.Append("<script src='https://cdn.jsdelivr.net/npm/summernote@0.8.18/dist/summernote-lite.min.js'></script>");
            }
            if (this.UseDataTable)
            {
                if (this.OnTop)
                    stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/dataTables.bootstrap4.min.css' />");
                if (this.OnBottom)
                {
                    stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/jquery.dataTables.min.js'></script>");
                    stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/dataTables.bootstrap4.min.js'></script>");
                }
            }
            if (this.DefaultUI != null)
            {
                if (this.OnTop)
                {
                    stringBuilder.Append(this.DefaultUI.ToCssVariables());
                    stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/backoffice/sb-admin-2.min.css' />");
                }
                if (this.OnBottom)
                    stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/backoffice/sb-admin-2.min.js'></script>");
            }
            if (this.OnTop)
                stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.css' />");
            if (this.OnBottom)
                stringBuilder.Append($"<script src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.js'></script>");
            string returnValue = stringBuilder.ToString();
            if (this.Cache)
                JavascriptCssCache.Add(this.OnTop, returnValue);
            output.Content.SetHtmlContent(returnValue);
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
