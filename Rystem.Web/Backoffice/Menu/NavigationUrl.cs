using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationUrl
    {
        public string Action { get; }
        public string Controller { get; }
        public string Area { get; }
        public NavigationUrl(string controller = "Home", string action = "Index", string area = "")
        {
            this.Action = action;
            this.Controller = controller;
            this.Area = area;
        }
    }
}