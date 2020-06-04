using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

//spinners: https://freefrontend.com/css-spinners/
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
        [HtmlAttributeName("rystem-cache")]
        public bool Cache { get; set; } = true;
        [HtmlAttributeName("jquery-datatable")]
        public bool UseDataTable { get; set; } = true;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private static string CssAndScriptsCache = string.Empty;
        private const string Script = "Rystem.jUrls.push({0});";
        private string AddScript(params string[] scriptsUri)
            => string.Format(Script, $"[{string.Join(",", scriptsUri.Where(x => x != null).Select(x => $"'{x}'"))}]");
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "rystem";
            if (this.Cache && !string.IsNullOrWhiteSpace(CssAndScriptsCache))
            {
                output.Content.SetHtmlContent(CssAndScriptsCache);
                return Task.CompletedTask;
            }
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder javascriptBuilder = new StringBuilder();
            javascriptBuilder.Append("<script>function rystemAsyncLoad(){");
            if (this.UseJQueryCoreCdn)
                stringBuilder.Append("<script src='https://code.jquery.com/jquery-3.5.1.min.js' integrity='sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=' crossorigin='anonymous'></script>");
            if (this.UseJQueryUiCdn != JQueryUIType.None)
            {
                javascriptBuilder.Append(AddScript("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"));
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
                javascriptBuilder.Append(AddScript(
                    "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.min.js",
                    $"{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/bootstrap/bootstrap-select.js",
                    Culture != CultureInfo.InvariantCulture ?
                        $"{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystembootstrap/i18n/defaults-{this.Culture.Name}.js"
                        : null));
            }
            if (this.UseSwiperCdn)
            {
                stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/Swiper/5.4.0/css/swiper.min.css' />");
                javascriptBuilder.Append(AddScript("https://cdnjs.cloudflare.com/ajax/libs/Swiper/5.4.0/js/swiper.min.js"));
            }
            if (this.UseFontAwesomeCdn)
            {
                stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.13.0/css/all.min.css' />");
            }
            if (this.UseMomentJsCdn)
            {
                javascriptBuilder.Append(AddScript("https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.25.3/moment.min.js"));
            }
            if (this.UseCharJsCdn)
            {
                stringBuilder.Append("<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.3/Chart.min.css' />");
                javascriptBuilder.Append(AddScript("https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.3/Chart.min.js"));
            }
            if (this.UseDataTable)
            {
                stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/dataTables.bootstrap4.min.css' />");
                javascriptBuilder.Append(AddScript(
                    $"{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/jquery.dataTables.min.js",
                    $"{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/datatables/dataTables.bootstrap4.min.js",
                    "$(\".rystemtable\").DataTable();"));
            }
            if (this.DefaultUI != null)
            {
                stringBuilder.Append(this.DefaultUI.ToCssVariables());
                stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/backoffice/sb-admin-2.min.css' />");
                javascriptBuilder.Append(AddScript($"{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/backoffice/sb-admin-2.min.js"));
            }
            javascriptBuilder.Append("Rystem.loadAsyncJavascript();}</script>");
            stringBuilder.Append($"<link rel='stylesheet' href='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.css' />");
            stringBuilder.Append($"<script onload='rystemAsyncLoad()' src='{this.ViewContext.HttpContext.Request.Scheme}://{this.ViewContext.HttpContext.Request.Host}/rystem/rystem.js'></script>");
            output.Content.SetHtmlContent(CssAndScriptsCache = $"{javascriptBuilder}{stringBuilder}");
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
