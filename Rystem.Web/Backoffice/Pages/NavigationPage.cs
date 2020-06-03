using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public abstract class NavigationPage<T>
        where T : class
    {
        public Navigation<T> Navigation { get; }
        private protected NavigationPage(Navigation<T> navigation)
            => this.Navigation = navigation;
    }
}