using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    public class EnumerableConverter : Converter
    {
        public EnumerableConverter(IConverterFactory factory, int index) : base(factory, index) { }

        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
                return new DictionaryConverter(this.Factory, this.Index).Deserialize(type, value, antiAbstractionInterfaceDictionary);
            IList list = (IList)Activator.CreateInstance(type);
            Type genericType = type.GetGenericArguments().First();
            foreach (string val in value.Split(ConverterConstant.Enumerable))
                list.Add(this.Factory.GetConverter(genericType, this.Index).Deserialize(genericType, val, antiAbstractionInterfaceDictionary));
            return list;
        }

        internal override string Serialize(object values, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            if (values is IDictionary)
                return new DictionaryConverter(this.Factory, this.Index).Serialize(values, abstractionInterfaceDictionary);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object value in values as IEnumerable)
            {
                stringBuilder.Append($"{this.Factory.GetConverter(value.GetType(), this.Index).Serialize(value, abstractionInterfaceDictionary)}{ConverterConstant.Enumerable}");
            }
            return stringBuilder.ToString().Trim(ConverterConstant.Enumerable);
        }
    }
}
