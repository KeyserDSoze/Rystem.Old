using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    internal class StartConversion : Converter
    {
        private static readonly IConverterFactory StaticFactory = new SerializerFactory();
        public StartConversion() : base(StaticFactory, 0, new Dictionary<string, string>()) { }
        internal override dynamic Deserialize(Type type, string value)
        {
            string[] values = value.Split(ConverterConstant.AbstractionInterfaceDictionary);
            if (values.Length > 1)
                this.AbstractionInterfaceMapping = (new DictionaryConverter(this.Factory, this.Index, this.AbstractionInterfaceMapping).Deserialize(typeof(Dictionary<string, string>), values[1]) as IDictionary<string, string>).ToDictionary(x => x.Value, x => x.Key);
            return this.HelpToDeserialize(type, values[0]);
        }
        internal override string Serialize(object value)
        {
            string returnValue = this.HelpToSerialize(value.GetType(), value);
            if (this.AbstractionInterfaceMapping.Count > 0)
                return $"{returnValue}{ConverterConstant.AbstractionInterfaceDictionary}{new DictionaryConverter(this.Factory, 0, this.AbstractionInterfaceMapping).Serialize(this.AbstractionInterfaceMapping)}";
            else
                return returnValue;
        }
    }
}
