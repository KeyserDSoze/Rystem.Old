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
    public sealed class PropertyOptions
    {
        public OutputType OutputType { get; set; }
        public bool IsLocalized { get; set; }
        public bool IsKey { get; set; }
        public string Format { get; set; }
        public PropertyOptions(OutputType outputType = OutputType.String, bool isLocalized = false, bool isKey = false, string format = null)
        {
            this.OutputType = outputType;
            this.IsLocalized = isLocalized;
            this.IsKey = isKey;
        }
        public static PropertyOptions Key { get; } = new PropertyOptions(OutputType.String, true, true);
        public static PropertyOptions Count { get; } = new PropertyOptions(OutputType.Count, true);
    }
}
