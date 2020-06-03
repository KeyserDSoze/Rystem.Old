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
        NavigationIndex<T> ToIndex(IEnumerable<T> values);
        NavigationDelete<T> ToDelete(T value);
    }
    public class IncludingNavigation<T, TPreviousProperty> : INavigation<T>
        where T : class
    {
        public Navigation<T> Navigation { get; }
        public IncludingNavigation(Navigation<T> navigation)
        {
            this.Navigation = navigation;
        }
        private IncludingNavigation<T, TProperty> InternalThenInclude<TProperty>(dynamic body, PropertyOptions options)
            => this.Navigation.SetSon<TProperty>(body.Member, options);
        public IncludingNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludingNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludingNavigation<T, TProperty> ThenInclude<TProperty>(Expression<Func<TPreviousProperty, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalThenInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => this.Navigation.Include(navigationPropertyPath, options);
        public NavigationIndex<T> ToIndex(IEnumerable<T> values)
        {
            this.Navigation.Values = values;
            return new NavigationIndex<T>(this.Navigation);
        }
        public NavigationDelete<T> ToDelete(T value)
        {
            this.Navigation.Values = new List<T>() { value };
            return new NavigationDelete<T>(this.Navigation);
        }
    }
}