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
        [HtmlAttributeName("rystem-value")]
        public string Value { get; set; }
        [HtmlAttributeName("rystem-options")]
        public SummerNote Options { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).ToResult();
        }
        private string Id { get; } = $"rystem-textarea-{Guid.NewGuid():N}";
        private const string TextAreaScript = "<script>$(document).ready(function() {{$('#{0}').summernote({1});}});</script>";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            if (context.AllAttributes["class"] != null)
                output.Attributes.Remove(context.AllAttributes["class"]);
            StringBuilder optionBuilder = new StringBuilder();
            if (this.Options == null)
                this.Options = SummerNote.Default;
            optionBuilder.Append($"{{height: {this.Options.Height}, toolbar: [");
            if (this.Options.HasStyle)
                optionBuilder.Append("['style', ['style']],");
            if (this.Options.HasFont)
                optionBuilder.Append("['font', ['bold', 'italic', 'underline', 'clear']],['font', ['fontsize', 'color', 'fontname']],['font', ['strikethrough', 'superscript', 'subscript']],");
            if (this.Options.HasParagraph)
                optionBuilder.Append("['para', ['paragraph', 'ol', 'ul', 'paragraph', 'height']],");
            if (this.Options.HasInsertion)
                optionBuilder.Append("['insert', ['link', 'picture', 'table', 'video', 'hr']],");
            if (this.Options.HasMiscellaneous)
                optionBuilder.Append("['misc', ['codeview', 'fullscreen', 'help']]");
            output.Content.AppendHtml($"<textarea id='{this.Id}' name='{this.Name}' class='{context.AllAttributes["class"]?.Value} rystem-textarea'>{this.Value}</textarea>");
            output.Content.AppendHtml(string.Format(TextAreaScript, this.Id, $"{optionBuilder.ToString().Trim(',')}]}}"));
            return Task.CompletedTask;
        }
    }
}
