using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public sealed class NavigationValue
    {
        public List<string> Values { get; } = new List<string>();
        public string Key { get; internal set; }
    }
}
