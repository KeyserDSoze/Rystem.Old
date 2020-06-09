using Microsoft.Extensions.Localization;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public enum PropertyType
    {
        Normal,
        Dropdown,
    }
    public class PropertyOptions
    {
        public OutputType OutputType { get; set; }
        public bool IsLocalized { get; set; }
        public bool IsKey { get; set; }
        public bool IsHidden { get; set; }
        public bool IsReadOnly { get; set; }
        public string Format { get; set; }
        public virtual PropertyType Type => PropertyType.Normal;
        public PropertyOptions(OutputType outputType = OutputType.String, bool isLocalized = false, bool isKey = false, string format = null, bool isHidden = false, bool isReadOnly = false)
        {
            this.OutputType = outputType;
            this.IsLocalized = isLocalized;
            this.IsKey = isKey;
            this.Format = format;
            this.IsHidden = isHidden;
            this.IsReadOnly = isReadOnly;
        }
        public static PropertyOptions Key { get; } = new PropertyOptions(OutputType.String, true, true);
        public static PropertyOptions Count { get; } = new PropertyOptions(OutputType.Count, true);
    }
    public class DropdownPropertyOptions : PropertyOptions
    {
        public DropdownHelper DropdownHelper { get; set; }

        public override PropertyType Type => PropertyType.Dropdown;

        public DropdownPropertyOptions(OutputType outputType = OutputType.String, bool isLocalized = false, bool isKey = false, string format = null) : base(outputType, isLocalized, isKey, format)
        {
        }
    }
}