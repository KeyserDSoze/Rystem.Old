﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Rystem.Web
{
    [HtmlTargetElement("rystem-dropdown", Attributes = "rystem-data", TagStructure = TagStructure.NormalOrSelfClosing)]
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

        private string Id { get; set; } = $"rystem-dropdown-{Guid.NewGuid():N}";
        public override void Process(TagHelperContext context, TagHelperOutput output)
            => ProcessAsync(context, output).ToResult();
        private const string EmptyFunction = "undefined";
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string id = context.AllAttributes["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
                this.Id = id;
            output.TagName = "select";
            output.Attributes.Add("id", this.Id);
            output.Attributes.Add("class", $"{context.AllAttributes["class"]} selectpicker");
            if (HasSearch)
                output.Attributes.Add("data-live-search", "true");
            if (IsMultiple)
                output.Attributes.Add("multiple", "multiple");
            if (!string.IsNullOrEmpty(DataHeader))
                output.Attributes.Add("data-header", DataHeader);
            output.Attributes.Add("data-width", GetSize());
            var group = this.Data.GroupBy(x => x.Group);
            if (MaxSelected > 0 && group.Count() <= 1)
                output.Attributes.Add("data-max-options", MaxSelected.ToString());
            if (Sorting == SortType.Ascending)
                group = group.OrderBy(x => x.Key);
            else if (Sorting == SortType.Descending)
                group = group.OrderByDescending(x => x.Key);
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
            output.PostContent.AppendHtml(string.Format(StarterScript,
                    this.Id,
                    this.RequestContext?.FinalizeRequestContext(ViewContext.HttpContext.Request) ?? EmptyFunction,
                    this.UpdateRequestContext?.FinalizeRequestContext(this.ViewContext.HttpContext.Request) ?? EmptyFunction));

            return Task.CompletedTask;
        }
        private const string StarterScript = "<script>new DropdownRystem('{0}', {1}, {2}).show();</script>";
        private string GetSize()
        {
            switch (this.Size)
            {
                default:
                case SizeType.Medium:
                    return "50%";
                case SizeType.Small:
                    return "25%";
                case SizeType.Large:
                    return "75%";
                case SizeType.ExtraLarge:
                    return "100%";
            }
        }
    }
}