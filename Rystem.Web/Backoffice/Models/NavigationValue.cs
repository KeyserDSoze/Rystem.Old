using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public sealed class NavigationValue
    {
        public List<NavigationObject> Elements { get; set; } = new List<NavigationObject>();
        public string Key { get; internal set; }
    }
    public class NavigationObject
    {
        public string Value { get; }
        public PropertyOptions Options { get; }
        public IEnumerable<object> BaseObjects { get; }
        public NavigationObject(string value, PropertyOptions options, IEnumerable<object> baseObjects)
        {
            this.Value = value;
            this.Options = options;
            this.BaseObjects = baseObjects;
        }
    }
}