using Rystem.Conversion;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    internal class ObjectConverter : Converter
    {
        public ObjectConverter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping) : base(factory, index, abstractionInterfaceMapping) { }
        private static readonly Type Ignore = typeof(CsvIgnore);
        internal override string Serialize(object value)
        {
            if (value == null)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PropertyInfo property in Properties.Fetch(value.GetType(), Ignore))
                stringBuilder.Append($"{this.HelpToSerialize(property.PropertyType, property.GetValue(value))}{this.IndexAsChar}");
            return stringBuilder.ToString().Trim(this.IndexAsChar);
        }

        internal override dynamic Deserialize(Type type, string value)
        {
            if (value == null)
                return default;
            dynamic startValue = Activator.CreateInstance(type);
            int count = 0;
            string[] values = value.Split(this.IndexAsChar);
            foreach (PropertyInfo property in Properties.Fetch(type, Ignore))
            {
                if (count >= values.Length)
                    break;
                property.SetValue(startValue, this.HelpToDeserialize(property.PropertyType, values[count++]));
            }
            return startValue;
        }
    }
}
