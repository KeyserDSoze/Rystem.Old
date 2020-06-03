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
        public IEnumerable<T> Values { get; }
        public Navigation(IEnumerable<T> values, NavigationOptions options)
        {
            this.Values = values;
            this.Options = options;
        }
        public Navigation(T value, NavigationOptions options) 
            : this(new List<T>() { value }, options)
        {
        }
        internal List<Property> Properties { get; } = new List<Property>();
        public NavigationProperty<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath, PropertyOptions options = null)
        {
            dynamic body = navigationPropertyPath.Body;
            this.Properties.Add(new Property(body.Member, options));
            this.Last = this.Properties.Last();
            return new NavigationProperty<T, TProperty>(this);
        }
        public NavigationProperty<T, TProperty> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, PropertyOptions options = null)
        {
            dynamic body = navigationPropertyPath.Body;
            this.Properties.Add(new Property(body.Member, options));
            this.Last = this.Properties.Last();
            return new NavigationProperty<T, TProperty>(this);
        }
        private Property Last;
        internal NavigationProperty<T, TNewProperty> SetSon<TNewProperty>(PropertyInfo propertyInfo, PropertyOptions options)
        {
            this.Last.Properties.Add(new Property(propertyInfo, options, this.Last));
            this.Last = this.Last.Properties.Last();
            return new NavigationProperty<T, TNewProperty>(this);
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
                yield return this.Options.GetLocalizedString(property.PropertyInfo.Name);
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
