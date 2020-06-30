using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class DropDownNavigation<T> : PropertyOptions
        where T : class
    {
        public bool CreateInModal { get; set; }
        public RequestContext RequestContext { get; set; }
        public RequestContext UpdateRequestContext { get; set; }
        public Func<IEnumerable<DropdownItem>, T> GetData { get; set; }
        public Func<IEnumerable<DropdownItem>, IEnumerable<T>> GetDataFromEnumerable { get; set; }
        public IEnumerable<string> DisabledValues { get; set; }
        public string DataHeader { get; set; }
        public bool IsMultiple { get; set; }
        public bool SelectedFirst { get; set; }
        public SortType Sorting { get; set; }
        public bool HasSearch { get; set; } = true;
        public int MaxSelected { get; set; }
        public SizeType Size { get; set; }
        public string Name { get; set; }
        public string ItemName { get; set; } = ItemNameBase;
        private const string ItemNameBase = "selectedItems";
        public override PropertyType Type => PropertyType.Dropdown;
        public DropDownNavigation(OutputType outputType = OutputType.String, bool isLocalized = false, bool isKey = false, string format = null, bool isHidden = false, bool isReadOnly = false) : base(outputType, isLocalized, isKey, format, isHidden, isReadOnly)
        {
        }
    }
}