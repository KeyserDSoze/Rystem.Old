using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public class NavigationManager<T>
        where T : class
    {
        private static readonly NavigationOptions Default = new NavigationOptions();
        public Navigation<T> Create(IEnumerable<T> entity, NavigationOptions options = null)
            => new Navigation<T>(entity, options ?? Default);
        public Navigation<T> Create(T entity, NavigationOptions options = null)
            => new Navigation<T>(entity, options ?? Default);
    }
}
