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
    internal sealed class PropertyObjectWrapper
    {
        public object Value { get; set; }
        public string Format { get; set; }
        public override string ToString()
            => this.Format != null && Value != null ? (Value as dynamic).ToString(Format) : Value?.ToString();
    }
}
