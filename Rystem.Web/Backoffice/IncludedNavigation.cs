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
    public class IncludedNavigation<T, TPreviousProperty> : INavigation<T>
        where T : class
    {
        public Navigation<T> Navigation { get; }
        internal IncludedNavigation(Navigation<T> navigation)
        {
            this.Navigation = navigation;
        }
        private IncludedNavigation<T, TProperty> InternalThenInclude<TProperty>(dynamic body, PropertyOptions options)
            => this.Navigation.SetSon<TProperty>(body.Member, options);
        public IncludedNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludedNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludedNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
    }
}