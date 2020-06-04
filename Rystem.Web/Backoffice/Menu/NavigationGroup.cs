using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationGroup : NavigationItem
    {
        internal NavigationGroup(string title, NavigationBar navigationBar, NavigationUrl navigationUrl, string fontAwesomeIconClass, params string[] roles) : base(title, navigationBar, navigationUrl, fontAwesomeIconClass, roles)
        {
        }

        public override bool IsGroup => true;
        public List<NavigationItem> Items { get; } = new List<NavigationItem>();
        public NavigationGroup AddItem(string title, string fontAwesomeIconClass = null, NavigationUrl navigationUrl = null, params string[] roles)
        {
            this.Items.Add(new NavigationItem(title, this.NavigationBar, navigationUrl, fontAwesomeIconClass, roles));
            return this;
        }
        public NavigationBuilder Build()
           => new NavigationBuilder(this.NavigationBar);
    }
}