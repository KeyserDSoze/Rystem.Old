using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Rystem.Web.Backoffice;
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
        [HtmlAttributeName("rystem-headers")]
        public IEnumerable<string> Headers { get; set; }
        [HtmlAttributeName("rystem-values")]
        public IEnumerable<NavigationValue> Values { get; set; }
        [HtmlAttributeName("rystem-can-modify")]
        public bool CanModify { get; set; }
        [HtmlAttributeName("rystem-can-delete")]
        public bool CanDelete { get; set; }
        [HtmlAttributeName("rystem-modify-request")]
        public RoutingContext ModifyRequest { get; set; }
        [HtmlAttributeName("rystem-delete-request")]
        public RoutingContext DeleteRequest { get; set; }
        private string Id { get; } = $"rystem-datepicker-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string TableScript = "<script>$(document).ready(function() {{new TableRystem('{0}', '{1}').show();}});</script>";
        private const string Header = "<thead><tr>{0}</tr></thead>";
        private const string HeaderWithDelete = "<thead><tr>{0}<td></td></tr></thead>";
        private const string BodyElement = "<td>{0}</td>";
        private const string Trash = "<td style='cursor:pointer;' onclick=\"document.location = '{0}';\"><i class=\"fa fa-trash\" aria-hidden=\"true\"></i></td>";
        private const string ValueElement = "{0}";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";
            output.Attributes.Add("id", this.Id);
            output.Content.AppendHtml(string.Format(this.CanDelete ? HeaderWithDelete : Header, string.Join("", Headers.Select(x => $"<td>{x}</td>"))));
            output.Content.AppendHtml("<tbody>");
            foreach (NavigationValue navigationValue in Values)
            {
                output.Content.AppendHtml($"<tr>");
                string valueBase = ValueElement;
                if (this.CanModify)
                {
                    this.ModifyRequest.FurtherPath = navigationValue.Key;
                    valueBase = $"<a style='cursor:pointer;' href='{this.ModifyRequest.GetUrl(ViewContext.HttpContext.Request)}'>{{0}}</a>";
                }
                foreach (var value in navigationValue.Values)
                {
                    output.Content.AppendHtml(string.Format(BodyElement, string.Format(valueBase, value)));
                }
                if (this.CanModify)
                    output.Content.AppendHtml("</a>");
                if (this.CanDelete)
                {
                    this.DeleteRequest.FurtherPath = navigationValue.Key;
                    output.Content.AppendHtml(string.Format(Trash, this.DeleteRequest.GetUrl(ViewContext.HttpContext.Request)));
                }
                output.Content.AppendHtml("</tr>");
            }
            output.Content.AppendHtml("</tbody>");
            output.PostContent.AppendHtml(string.Format(TableScript, this.Id, GetLanguage()));
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