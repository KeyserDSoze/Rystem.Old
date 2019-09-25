using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    internal class EnumerableConverter : Converter
    {
        public EnumerableConverter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping) : base(factory, index, abstractionInterfaceMapping) { }

        internal override dynamic Deserialize(Type type, string value)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
                return new DictionaryConverter(this.Factory, this.Index, this.AbstractionInterfaceMapping).Deserialize(type, value);
            IList list = (IList)Activator.CreateInstance(type);
            Type genericType = type.GetGenericArguments().First();
            foreach (string val in value.Split(this.Enumerable))
                list.Add(this.HelpToDeserialize(genericType, val));
            return list;
        }

        internal override string Serialize(object values)
        {
            if (values is IDictionary)
                return new DictionaryConverter(this.Factory, this.Index, this.AbstractionInterfaceMapping).Serialize(values);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object value in values as IEnumerable)
                stringBuilder.Append($"{this.HelpToSerialize(value.GetType(), value)}{this.Enumerable}");
            return stringBuilder.ToString().Trim(this.Enumerable);
        }
    }
}
