using Microsoft.Extensions.Localization;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public interface INavigation<T>
        where T : class
    {
        Navigation<T> Navigation { get; }
        private static readonly NavigationOptions Default = new NavigationOptions();
        public static Navigation<T> Create(NavigationOptions options = null)
            => new Navigation<T>(options ?? Default);
    }
}