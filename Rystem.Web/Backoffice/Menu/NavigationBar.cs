using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationBar
    {
        public IStringLocalizer Localizer { get; private set; }
        public bool IsLocalized => this.Localizer != null;
        public Func<string, bool> UserIsInRole { get; private set; }
        public string WhiteLabel { get; }
        public string FontAwesomeIconClass { get; }
        public List<NavigationSuperGroup> SuperGroups { get; } = new List<NavigationSuperGroup>();
        public NavigationBar(string whiteLabel, string fontAwesomeIconClass)
        {
            this.WhiteLabel = whiteLabel;
            this.FontAwesomeIconClass = fontAwesomeIconClass;
        }
        public NavigationGroup AddSuperGroup(string title = null, string fontAwesomeIconClass = null, NavigationUrl navigationUrl = null, params string[] roles)
        {
            if (this.SuperGroups.Count > 0)
                this.SuperGroups.Last().Last = false;
            this.SuperGroups.Add(new NavigationSuperGroup(title, this, navigationUrl, fontAwesomeIconClass, roles));
            return this.SuperGroups.Last();
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
}