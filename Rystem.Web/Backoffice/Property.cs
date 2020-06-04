using Microsoft.Extensions.Localization;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rystem.Web.Backoffice
{
    internal sealed class Property
    {
        public PropertyInfo PropertyInfo { get; }
        public List<Property> Properties { get; }
        public PropertyOptions Options { get; }
        private static readonly PropertyOptions DefaultOptions = new PropertyOptions();
        private readonly List<Property> Tree = new List<Property>();
        public Property(PropertyInfo propertyInfo, PropertyOptions options, Property father = null)
        {
            this.PropertyInfo = propertyInfo;
            this.Options = options ?? DefaultOptions;
            this.Properties = new List<Property>();
            if (father != null)
                this.Tree.AddRange(father.Tree);
            this.Tree.Add(this);
        }
        public string FullName
        {
            get
            {
                DisplayAttribute displayAttribute = this.PropertyInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    return displayAttribute.Name;
                return string.Join(".", this.Tree.Select(x => x.PropertyInfo.Name));
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            if (this.Properties.Count == 0)
                yield return this;
            else
                foreach (Property property in this.Properties)
                    foreach (Property pi in property.GetAllProperties())
                        yield return pi;
        }
        public string FromObject(object entity)
        {
            List<PropertyObjectWrapper> result = new List<PropertyObjectWrapper>() { new PropertyObjectWrapper() { Value = entity } };
            foreach (Property property in this.Tree)
            {
                List<PropertyObjectWrapper> innerResult = new List<PropertyObjectWrapper>();
                if (!Primitive.Is(property.PropertyInfo.PropertyType) && typeof(IEnumerable).IsAssignableFrom(property.PropertyInfo.PropertyType))
                {
                    foreach (PropertyObjectWrapper wrapper in result)
                    {
                        int count = 0;
                        foreach (object value in property.PropertyInfo.GetValue(wrapper.Value) as IEnumerable)
                        {
                            if (property.Options.OutputType == OutputType.String)
                                innerResult.Add(new PropertyObjectWrapper() { Value = value, Format = property.Options.Format });
                            else if (property.Options.OutputType == OutputType.Count)
                                count++;
                        }
                        if (property.Options.OutputType == OutputType.Count)
                            innerResult.Add(new PropertyObjectWrapper() { Value = count });
                    }
                    result = innerResult;
                }
                else
                {
                    foreach (PropertyObjectWrapper wrapper in result)
                    {
                        wrapper.Value = wrapper.Value != null ? property.PropertyInfo.GetValue(wrapper.Value) : null;
                        wrapper.Format = property.Options.Format;
                    }
                }
            }
            return string.Join(",", result);
        }
    }
}