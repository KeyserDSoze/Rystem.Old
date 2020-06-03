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
        public NavigationUrl(string action, string controller = null, string area = null)
        {
            this.Action = action;
            this.Controller = controller;
            this.Area = area;
        }
    }
    public class NavigationBar
    {
        public IStringLocalizer Localizer { get; private set; }
        public bool IsLocalized => this.Localizer != null;
        public Func<string, bool> UserIsInRole { get; private set; }
        public string WhiteLabel { get; }
        public string FontAwesomeIconClass { get; }
        public List<NavigationGroup> Groups { get; } = new List<NavigationGroup>();
        public NavigationBar(string whiteLabel, string fontAwesomeIconClass)
        {
            this.WhiteLabel = whiteLabel;
            this.FontAwesomeIconClass = fontAwesomeIconClass;
        }
        public NavigationGroup AddGroup(string title, NavigationUrl navigationUrl = null, string fontAwesomeIconClass = null, params string[] roles)
        {
            this.Groups.Add(new NavigationGroup(title, this, navigationUrl, fontAwesomeIconClass, roles));
            return this.Groups.Last();
        }
        internal bool IsVisible(string[] entryRoles)
        {
            if (entryRoles == null)
                return true;
            else
            {
                bool isInRole = true;
                foreach (string role in entryRoles)
                {
                    string[] roles = role.Split(',');
                    bool check = false;
                    foreach (string innerRole in roles)
                    {
                        if (this.UserIsInRole(innerRole))
                        {
                            check = true;
                            break;
                        }
                    }
                    isInRole &= check;
                }
                return isInRole;
            }
        }

        internal NavigationBar SetLocalizationAndAccount(IStringLocalizer localizer = null, Func<string, bool> userIsInRole = null)
        {
            this.Localizer = localizer;
            this.UserIsInRole = userIsInRole;
            return this;
        }
    }
    public class NavigationGroup : NavigationItem
    {
        internal NavigationGroup(string title, NavigationBar navigationBar, NavigationUrl navigationUrl, string fontAwesomeIconClass, params string[] roles) : base(title, navigationBar, navigationUrl, fontAwesomeIconClass, roles)
        {
        }

        public override bool IsGroup => true;
        public List<NavigationItem> Items { get; } = new List<NavigationItem>();
        public NavigationGroup AddItem(string title, NavigationUrl navigationUrl = null, string fontAwesomeIconClass = null, params string[] roles)
        {
            this.Items.Add(new NavigationItem(title, this.NavigationBar, navigationUrl, fontAwesomeIconClass, roles));
            return this;
        }
        public NavigationBar Build(IStringLocalizer localizer = null, Func<string, bool> userIsInRole = null)
           => this.NavigationBar.SetLocalizationAndAccount(localizer, userIsInRole);
    }
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
        internal bool IsVisible()
            => this.NavigationBar.IsVisible(this.Roles);
        public NavigationGroup AddGroup(string title)
           => this.NavigationBar.AddGroup(title);
    }
}