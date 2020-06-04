using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationBuilder
    {
        private readonly NavigationBar NavigationBar;
        internal NavigationBuilder(NavigationBar navigationBar)
            => this.NavigationBar = navigationBar;
        public NavigationBar Run(IStringLocalizer localizer = null, Func<string, bool> userIsInRole = null)
                   => this.NavigationBar.SetLocalizationAndAccount(localizer, userIsInRole);
    }
}