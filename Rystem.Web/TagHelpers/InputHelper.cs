using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
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
    [HtmlTargetElement("rystem-input", Attributes = "rystem-value", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class InputHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-value")]
        public object Value { get; set; }
        [HtmlAttributeName("rystem-localization")]
        public IStringLocalizer Localizer { get; set; }
        [HtmlAttributeName("rystem-hidden")]
        public bool IsHidden { get; set; }
        [HtmlAttributeName("rystem-readonly")]
        public bool IsReadOnly { get; set; }
        public bool IsLocalized => Localizer != null;
        private string Id { get; } = $"rystem-input-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private const string Label = "<label class='control-label' for='{0}'>{1}</label>";
        private const string Input = "<input class='form-control valid' type='{0}' data-val='true' id='{1}' name='{1}' value='{2}' aria-describedby='{1}-error' aria-invalid='false' {3}>";
        private const string Validation = "<span class='text-danger field-validation-valid' data-valmsg-for='{0}' data-valmsg-replace='true'></span>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add("id", this.Id);
            Type type = this.Value.GetType();
            string name = type.Name;
            if (!this.IsHidden)
            {
                output.Content.AppendHtml(string.Format(Label, name, this.IsLocalized ? Localizer[name] : name));
                output.Content.AppendHtml(string.Format(Input, this.GetType(type), name, this.Value.ToString(), $"{(IsReadOnly ? "readonly" : string.Empty)} {this.GetValidation(type)}"));
                output.Content.AppendHtml(string.Format(Validation, name));
            }
            else
                output.Content.AppendHtml(string.Format(Input, "hidden", name, this.Value.ToString(), string.Empty));
            return Task.CompletedTask;
        }
        private string GetValidation(Type type) => string.Empty;
        private string GetType(Type type)
            => NormalTypes[type];
        private static readonly Dictionary<Type, string> NormalTypes = new Dictionary<Type, string>
        {
            { typeof(int), "number"},
            { typeof(bool), "checkbox" },
            { typeof(char), "text" },
            { typeof(decimal), "number" },
            { typeof(double),  "number" },
            { typeof(long), "number" },
            { typeof(float), "number" },
            { typeof(uint), "number" },
            { typeof(ulong), "number" },
            { typeof(short), "number" },
            { typeof(ushort), "number" },
            { typeof(string), "text" },
            { typeof(int?), "number"},
            { typeof(bool?), "checkbox" },
            { typeof(char?), "text" },
            { typeof(decimal?), "number" },
            { typeof(double?),  "number" },
            { typeof(long?), "number" },
            { typeof(float?), "number" },
            { typeof(uint?), "number" },
            { typeof(ulong?), "number" },
            { typeof(short?), "number" },
            { typeof(ushort?), "number" },
            { typeof(string), "text" },
            { typeof(Guid), "text" },
            { typeof(Guid?), "text" }
        };
    }
}