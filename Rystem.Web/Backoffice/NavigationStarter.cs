using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public static class NavigationStarter<T>
        where T : class
    {
        private static readonly NavigationOptions Default = new NavigationOptions();
        public static Navigation<T> Create(NavigationOptions options = null)
            => new Navigation<T>(options ?? Default);
    }
}