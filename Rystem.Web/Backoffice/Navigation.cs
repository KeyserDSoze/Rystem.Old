using Microsoft.Extensions.Localization;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace Rystem.Web.Backoffice
{
    public sealed class Navigation<T>
        where T : class
    {
        internal NavigationOptions Options { get; }
        internal Navigation(NavigationOptions options)
        {
            this.Options = options;
        }
        internal List<Property> Properties { get; } = new List<Property>();
        private IncludedNavigation<T, TProperty> InternalInclude<TProperty>(dynamic body, PropertyOptions options = null)
        {
            this.Properties.Add(new Property(body.Member, options));
            this.Last = this.Properties.Last();
            return new IncludedNavigation<T, TProperty>(this);
        }
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludedNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        private Property Last;
        internal IncludedNavigation<T, TNewProperty> SetSon<TNewProperty>(PropertyInfo propertyInfo, PropertyOptions options)
        {
            this.Last.Properties.Add(new Property(propertyInfo, options, this.Last));
            this.Last = this.Last.Properties.Last();
            return new IncludedNavigation<T, TNewProperty>(this);
        }
        private IEnumerable<Property> GetAllProperties()
        {
            foreach (var properties in this.Properties)
                foreach (var property in properties.GetAllProperties())
                    yield return property;
        }
        internal IEnumerable<(string Value, string Localized)> GetHeaders()
        {
            foreach (Property property in GetAllProperties())
                yield return (property.FullName, this.Options.GetLocalizedString(property.FullName));
        }
        internal NavigationValue GetValues(T entity)
        {
            NavigationValue navigationValue = new NavigationValue();
            navigationValue.Key = string.Empty;
            foreach (Property property in GetAllProperties())
            {
                var trueEntity = property.FromObject(entity, this.Options);
                navigationValue.Elements.Add(new NavigationObject(trueEntity.AsString, property.Options, trueEntity.AsObject.Select(x => x.Value)));
                if (property.Options.IsKey)
                {
                    if (navigationValue.Key.Length == 0)
                        navigationValue.Key = trueEntity.AsString;
                    else
                        navigationValue.Key += $"/{HttpUtility.UrlEncode(trueEntity.AsString)}";
                }
            }
            if (navigationValue.Key.Length == 0)
                navigationValue.Key = navigationValue.Elements.FirstOrDefault().Value;
            return navigationValue;
        }
    }
}