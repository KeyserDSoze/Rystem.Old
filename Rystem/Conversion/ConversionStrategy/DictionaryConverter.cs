using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal class DictionaryConverter : Converter
    {
        public DictionaryConverter(IConverterFactory factory, int index) : base(factory, index) { }

        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
            Type[] keyValueType = type.GetGenericArguments();
            foreach (string val in value.Split(ConverterConstant.Enumerable))
            {
                string[] keyValue = val.Split(ConverterConstant.Dictionarable);
                dictionary.Add(
                    this.Factory.GetConverter(keyValueType[0], this.Index).Deserialize(keyValueType[0], keyValue[0], antiAbstractionInterfaceDictionary),
                    this.Factory.GetConverter(keyValueType[1], this.Index).Deserialize(keyValueType[1], keyValue[1], antiAbstractionInterfaceDictionary)
                    );
            }
            return dictionary;
        }

        internal override string Serialize(object values, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DictionaryEntry entry in values as IDictionary)
            {
                string key = this.Factory.GetConverter(entry.Key.GetType(), this.Index).Serialize(entry.Key, abstractionInterfaceDictionary);
                string value = this.Factory.GetConverter(entry.Value.GetType(), this.Index).Serialize(entry.Value, abstractionInterfaceDictionary);
                stringBuilder.Append($"{key}{ConverterConstant.Dictionarable}{value}{ConverterConstant.Enumerable}");
            }
            return stringBuilder.ToString().Trim(ConverterConstant.Enumerable);
        }
    }
}
