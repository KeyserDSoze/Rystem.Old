using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-dropdown", Attributes = "rystem-data,name", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class DropdownHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("rystem-request-context")]
        public RequestContext RequestContext { get; set; }
        [HtmlAttributeName("rystem-update-context")]
        public RequestContext UpdateRequestContext { get; set; }
        [HtmlAttributeName("rystem-data")]
        public IEnumerable<DropdownItem> Data { get; set; }
        [HtmlAttributeName("rystem-data-disabled")]
        public IEnumerable<string> DisabledValues { get; set; }
        [HtmlAttributeName("rystem-data-header")]
        public string DataHeader { get; set; }
        [HtmlAttributeName("rystem-data-multiple")]
        public bool IsMultiple { get; set; }
        [HtmlAttributeName("rystem-data-selectfirst")]
        public bool SelectedFirst { get; set; }
        [HtmlAttributeName("rystem-data-sorting")]
        public SortType Sorting { get; set; }
        [HtmlAttributeName("rystem-data-search")]
        public bool HasSearch { get; set; } = true;
        [HtmlAttributeName("rystem-data-maxselected")]
        public int MaxSelected { get; set; }
        [HtmlAttributeName("rystem-size")]
        public SizeType Size { get; set; }
        [HtmlAttributeName("name")]
        public string Name { get; set; }
        [HtmlAttributeName("rystem-item-name")]
        public string ItemName { get; set; } = ItemNameBase;
        private const string ItemNameBase = "selectedItems";
        private readonly string Id = $"rystem-dropdown-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
            => ProcessAsync(context, output).ToResult();
        private const string EmptyFunction = "undefined";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder stringBuilder = new StringBuilder();
            output.TagName = "div";
            if (context.AllAttributes["class"] != null)
                output.Attributes.Remove(context.AllAttributes["class"]);
            stringBuilder.Append($"<select id='{this.Id}' name='{this.Name}' class='{context.AllAttributes["class"]?.Value} selectpicker {this.Id}'");
            if (HasSearch)
                stringBuilder.Append(" data-live-search='true'");
            if (IsMultiple)
                stringBuilder.Append(" multiple");
            if (!string.IsNullOrEmpty(DataHeader))
                stringBuilder.Append($" data-header='{DataHeader}'");
            stringBuilder.Append($" data-width='{GetSize()}'");
            var group = this.Data.GroupBy(x => x.Group);
            if (MaxSelected > 0 && group.Count() <= 1)
                stringBuilder.Append($" data-max-options={MaxSelected}");
            stringBuilder.Append(">");
            output.Content.AppendHtml(stringBuilder.ToString());
            if (Sorting == SortType.Ascending)
                group = group.OrderBy(x => x.Key);
            else if (Sorting == SortType.Descending)
                group = group.OrderByDescending(x => x.Key);
            if (!IsMultiple)
                output.Content.AppendHtml($"<option value=''></option>");
            foreach (var list in group)
            {
                if (!string.IsNullOrWhiteSpace(list.Key))
                    output.Content.AppendHtml($"<optgroup label='{list.Key}' {(MaxSelected > 0 ? "data-max-options='2'" : string.Empty)}>");
                IEnumerable<DropdownItem> sortedList = list;
                if (Sorting == SortType.Ascending)
                    sortedList = list.OrderBy(x => x.Value);
                else if (Sorting == SortType.Descending)
                    sortedList = list.OrderByDescending(x => x.Value);
                foreach (var item in list)
                {
                    output.Content.AppendHtml($"<option data-icon='{item.Icon}' value='{item.Value}' data-tokens='{item.Group}' {IsSelected()} {IsDangerous()}>{item.Label}</option>");
                    string IsSelected()
                        => item.Selected ? "selected=selected" : string.Empty;
                    string IsDangerous()
                        => this.DisabledValues?.Any(x => x == item.Value) == true ? "disabled style='color:#b94a48;'" : string.Empty;
                }
                if (!string.IsNullOrWhiteSpace(list.Key))
                    output.Content.AppendHtml("</optgroup>");
            }
            output.Content.AppendHtml("</select>");
            output.PostContent.AppendHtml(string.Format(StarterScript,
                    this.Id,
                    this.ItemName,
                    this.RequestContext?.FinalizeRequestContext(ViewContext.HttpContext.Request) ?? EmptyFunction,
                    this.UpdateRequestContext?.FinalizeRequestContext(ViewContext.HttpContext.Request) ?? EmptyFunction));
            return Task.CompletedTask;
        }
        private const string StarterScript = "<script>$(document).ready(function() {{new DropdownRystem('{0}', '{1}', {2}, {3}).show();}});</script>";
        private string GetSize()
            => this.Size switch
            {
                SizeType.Small => "25%",
                SizeType.Large => "75%",
                SizeType.ExtraLarge => "100%",
                _ => "50%",
            };
    }
}
