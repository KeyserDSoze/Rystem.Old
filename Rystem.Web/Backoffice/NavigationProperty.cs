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
    public class NavigationProperty<T, TPreviousProperty>
        where T : class
    {
        public Navigation<T> Navigation { get; }
        public NavigationProperty(Navigation<T> navigation)
        {
            this.Navigation = navigation;
        }
        public NavigationProperty<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath, PropertyOptions options = null)
        {
            dynamic body = navigationPropertyPath.Body;
            return this.Navigation.SetSon<TProperty>(body.Member, options);
        }
        public NavigationProperty<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
        {
            dynamic body = navigationPropertyPath.Body;
            return this.Navigation.SetSon<TProperty>(body.Member, options);
        }
        public NavigationProperty<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public NavigationProperty<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public NavigationIndex<T> ToIndex()
            => new NavigationIndex<T>(this.Navigation);
    }
}
