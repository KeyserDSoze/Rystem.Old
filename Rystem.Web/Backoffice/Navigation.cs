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
    public sealed class Navigation<T>
        where T : class
    {
        internal NavigationOptions Options { get; }
        public IEnumerable<T> Values { get; internal set; }
        public Navigation(NavigationOptions options)
        {
            this.Options = options;
        }
        internal List<Property> Properties { get; } = new List<Property>();
        private IncludingNavigation<T, TProperty> InternalInclude<TProperty>(dynamic body, PropertyOptions options = null)
        {
            this.Properties.Add(new Property(body.Member, options));
            this.Last = this.Properties.Last();
            return new IncludingNavigation<T, TProperty>(this);
        }
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        public IncludingNavigation<T, TProperty> Include<TProperty>(Expression<Func<T, ICollection<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
            => InternalInclude<TProperty>(navigationPropertyPath.Body, options);
        private Property Last;
        internal IncludingNavigation<T, TNewProperty> SetSon<TNewProperty>(PropertyInfo propertyInfo, PropertyOptions options)
        {
            this.Last.Properties.Add(new Property(propertyInfo, options, this.Last));
            this.Last = this.Last.Properties.Last();
            return new IncludingNavigation<T, TNewProperty>(this);
        }
        private IEnumerable<Property> GetAllProperties()
        {
            foreach (var properties in this.Properties)
                foreach (var property in properties.GetAllProperties())
                    yield return property;
        }
        internal IEnumerable<string> GetHeaders()
        {
            foreach (Property property in GetAllProperties())
                yield return this.Options.GetLocalizedString(property.FullName);
        }
        internal NavigationValue GetValues(T entity)
        {
            NavigationValue navigationValue = new NavigationValue();
            foreach (Property property in GetAllProperties())
            {
                string value = property.FromObject(entity);
                if (property.Options.IsLocalized)
                    value = this.Options.GetLocalizedString(value);
                navigationValue.Values.Add(value);
                if (property.Options.IsKey)
                    navigationValue.Key = value;
            }
            if (navigationValue.Key == null)
                navigationValue.Key = navigationValue.Values.FirstOrDefault();
            return navigationValue;
        }
    }
}