using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal class DictionaryConverter : Converter
    {
        public DictionaryConverter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping) : base(factory, index, abstractionInterfaceMapping) { }

        internal override dynamic Deserialize(Type type, string value)
        {
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
            Type[] keyValueType = type.GetGenericArguments();
            foreach (string val in value.Split(this.Enumerable))
            {
                string[] keyValue = val.Split(this.Dictionarable);
                dictionary.Add(
                    this.HelpToDeserialize(keyValueType[0], keyValue[0]),
                    this.HelpToDeserialize(keyValueType[1], keyValue[1])
                    );
            }
            return dictionary;
        }

        internal override string Serialize(object values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DictionaryEntry entry in values as IDictionary)
            {
                string key = this.HelpToSerialize(entry.Key.GetType(), entry.Key);
                string value = this.HelpToSerialize(entry.Value.GetType(), entry.Value);
                stringBuilder.Append($"{key}{(this.Dictionarable)}{value}{(this.Enumerable)}");
            }
            return stringBuilder.ToString().Trim((this.Enumerable));
        }
    }
}
