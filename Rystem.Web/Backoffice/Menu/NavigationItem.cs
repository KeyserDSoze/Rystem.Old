using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationItem
    {
        public string Id { get; } = $"navitem-{Guid.NewGuid():N}";
        public virtual bool IsGroup => false;
        public NavigationUrl Url { get; }
        public string FontAwesomeIconClass { get; }
        public string[] Roles { get; }
        private string title;
        public string Title
        {
            get => this.NavigationBar.IsLocalized ? this.NavigationBar.Localizer[title] : title;
            private set => title = value;
        }
        private protected readonly NavigationBar NavigationBar;
        internal NavigationItem(string title, NavigationBar navigationBar, NavigationUrl navigationUrl, string fontAwesomeIconClass, params string[] roles)
        {
            this.Title = title;
            this.Roles = roles;
            this.NavigationBar = navigationBar;
            this.Url = navigationUrl;
            this.FontAwesomeIconClass = fontAwesomeIconClass;
        }
        public bool IsVisible()
            => this.NavigationBar.IsVisible(this.Roles);
        public NavigationGroup AddSuperGroup(string title = null, string fontAwesomeIconClass = null, NavigationUrl navigationUrl = null, params string[] roles)
           => this.NavigationBar.AddSuperGroup(title, fontAwesomeIconClass, navigationUrl, roles);
        public virtual NavigationGroup AddGroup(string title, string fontAwesomeIconClass = null, NavigationUrl navigationUrl = null, params string[] roles)
           => this.NavigationBar.SuperGroups.Last().AddGroup(title, fontAwesomeIconClass, navigationUrl, roles);
    }
}