using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-text-area", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TextAreaHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("name")]
        public string Name { get; set; }
        [HtmlAttributeName("value")]
        public string Value { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private string Id { get; } = $"rystem-textarea-{Guid.NewGuid():N}";
        private const string TextAreaScript = "<script>$('#{0}').summernote({{height: 500, toolbar: [['style', ['style']],['font', ['bold', 'italic', 'underline', 'clear']],['font', ['fontsize', 'color', 'fontname']],['font', ['strikethrough', 'superscript', 'subscript']],['para', ['paragraph', 'ol', 'ul', 'paragraph', 'height']],['insert', ['link', 'picture', 'table', 'video', 'hr']], ['misc', ['codeview', 'fullscreen', 'help']]]}});</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (context.AllAttributes["class"] != null)
                output.Attributes.Remove(context.AllAttributes["class"]);
            output.Content.AppendHtml($"<textarea id='{this.Id}' name='{this.Name}' class='{context.AllAttributes["class"]?.Value} rystem-textarea'>{this.Value}</textarea>");
            output.Content.AppendHtml(string.Format(TextAreaScript, this.Id));
            return Task.CompletedTask;
        }
    }
}
