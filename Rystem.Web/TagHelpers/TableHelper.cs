using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-table", Attributes = "", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TableHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-culture")]
        public CultureInfo CultureInfo { get; set; }
        private string Id { get; } = $"rystem-datepicker-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string TableScript = "<script>$(document).ready(function() {{new TableRystem('{0}', '{1}').show();}});</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";
            output.Attributes.Add("id", this.Id);
            output.PostContent.AppendHtml(string.Format(TableScript,this.Id,GetLanguage()));
            return Task.CompletedTask;
        }
        private const string EmptyFunction = "undefined";
        private string GetLanguage()
        {
            switch (this.CultureInfo?.Name.ToLower().Split('-').First())
            {
                default:
                    return EmptyFunction;
                case "it":
                    return "https://cdn.datatables.net/plug-ins/1.10.21/i18n/Italian.json";
                case "fr":
                    return "https://cdn.datatables.net/plug-ins/1.10.21/i18n/French.json";
                case "de":
                    return "https://cdn.datatables.net/plug-ins/1.10.21/i18n/German.json";
                case "es":
                    return "https://cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json";
                case "pt":
                    return "https://cdn.datatables.net/plug-ins/1.10.21/i18n/Portuguese.json";
            }
        }
    }
}
