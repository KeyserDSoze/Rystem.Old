using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    internal class StartConversion : Converter, ICsvInterpreter
    {
        public StartConversion() : base(0, new Dictionary<string, string>()) { }
        public dynamic Deserialize(Type type, string value)
        {
            string[] values = value.Split(ConverterConstant.AbstractionInterfaceDictionary);
            if (values.Length > 1)
                this.AbstractionInterfaceMapping = (new DictionaryConverter(this.Index, this.AbstractionInterfaceMapping).Deserialize(typeof(Dictionary<string, string>), values[1]) as IDictionary<string, string>).ToDictionary(x => x.Value, x => x.Key);
            return this.HelpToDeserialize(type, values[0]);
        }
        public string Serialize(object value)
        {
            string returnValue = this.HelpToSerialize(value.GetType(), value);
            if (this.AbstractionInterfaceMapping.Count > 0)
                return $"{returnValue}{ConverterConstant.AbstractionInterfaceDictionary}{new DictionaryConverter(0, this.AbstractionInterfaceMapping).Serialize(this.AbstractionInterfaceMapping)}";
            else
                return returnValue;
        }
    }
}
