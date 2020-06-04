using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationSuperGroup : NavigationGroup
    {
        public bool Last { get; internal set; }
        public List<NavigationGroup> Groups { get; } = new List<NavigationGroup>();
        internal NavigationSuperGroup(string title, NavigationBar navigationBar, NavigationUrl navigationUrl, string fontAwesomeIconClass, params string[] roles) : base(title, navigationBar, navigationUrl, fontAwesomeIconClass, roles)
        {
            this.Last = true;
        }
        public override NavigationGroup AddGroup(string title, string fontAwesomeIconClass = null, NavigationUrl navigationUrl = null, params string[] roles)
        {
            this.Groups.Add(new NavigationGroup(title, this.NavigationBar, navigationUrl, fontAwesomeIconClass, roles));
            return this.Groups.Last();
        }
    }
}