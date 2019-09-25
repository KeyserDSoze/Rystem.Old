using Rystem.Conversion;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    internal abstract class Converter
    {
        private protected IConverterFactory Factory;
        private protected int Index;
        public Converter(IConverterFactory factory, int index)
        {
            this.Factory = factory;
            this.Index = index;
        }
        public string StartSerialization(object value)
        {
            IDictionary<string, int> abstractionInterfaceDictionary = new Dictionary<string, int>();
            string returnValue = this.Serialize(value, abstractionInterfaceDictionary);
            if (abstractionInterfaceDictionary.Count > 0)
                return $"{returnValue}{ConverterConstant.AbstractionInterfaceDictionary}{new DictionaryConverter(this.Factory, this.Index).Serialize(abstractionInterfaceDictionary, abstractionInterfaceDictionary)}";
            else
                return returnValue;
        }
        internal abstract string Serialize(object value, IDictionary<string, int> abstractionInterfaceDictionary);
        internal abstract dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary);
        public dynamic StartDeserialization(Type type, string value)
        {
            string[] values = value.Split(ConverterConstant.AbstractionInterfaceDictionary);
            IDictionary<int, string> antiAbstractionInterfaceDictionary = new Dictionary<int, string>();
            if (values.Length > 1)
                antiAbstractionInterfaceDictionary = (new DictionaryConverter(this.Factory, this.Index).Deserialize(typeof(Dictionary<string, int>), values[1], antiAbstractionInterfaceDictionary) as IDictionary<string, int>).ToDictionary(x => x.Value, x => x.Key);
            return this.Deserialize(type, values[0], antiAbstractionInterfaceDictionary);
        }
    }
}
