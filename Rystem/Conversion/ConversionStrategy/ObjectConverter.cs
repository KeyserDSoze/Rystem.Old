using Rystem.Conversion;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    internal class ObjectConverter : Converter
    {
        public ObjectConverter(IConverterFactory factory, int index) : base(factory, index) { }
        private static readonly Type Ignore = typeof(CsvIgnore);
        internal override string Serialize(object value, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            if (value == null)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PropertyInfo property in Properties.Fetch(value.GetType(), Ignore))
                stringBuilder.Append($"{this.Factory.GetConverter(property.PropertyType, this.Index).Serialize(property.GetValue(value), abstractionInterfaceDictionary)}{(char)this.Index}");
            return stringBuilder.ToString().Trim((char)this.Index);
        }

        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            if (value == null)
                return default;
            dynamic startValue = Activator.CreateInstance(type);
            int count = 0;
            string[] values = value.Split((char)this.Index);
            foreach (PropertyInfo property in Properties.Fetch(type, Ignore))
            {
                if (count >= values.Length)
                    break;
                property.SetValue(startValue, this.Factory.GetConverter(property.PropertyType, this.Index).Deserialize(property.PropertyType, values[count], antiAbstractionInterfaceDictionary));
                count++;
            }
            return startValue;
        }
    }
}
