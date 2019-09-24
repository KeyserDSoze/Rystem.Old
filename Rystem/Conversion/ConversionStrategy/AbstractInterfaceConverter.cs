using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    public class AbstractInterfaceConverter : Converter
    {
        public AbstractInterfaceConverter(IConverterFactory factory, int index) : base(factory, index) { }
        private static string AbstractionInterface = ConverterConstant.AbstractionInterface.ToString();
        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            string[] values = value.Split(ConverterConstant.AbstractionInterface);
            Type toCreate = Type.GetType(antiAbstractionInterfaceDictionary[int.Parse(values[0])]);
            string remains = string.Join(AbstractionInterface, values.Skip(1));
            return this.Factory.GetConverter(toCreate, this.Index, true).Deserialize(toCreate, remains, antiAbstractionInterfaceDictionary);
        }

        internal override string Serialize(object value, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            StringBuilder builder = new StringBuilder();
            string assemblyName = value.GetType().AssemblyQualifiedName;
            if (!abstractionInterfaceDictionary.ContainsKey(assemblyName))
                abstractionInterfaceDictionary.Add(assemblyName, abstractionInterfaceDictionary.Count);
            builder.Append($"{abstractionInterfaceDictionary[assemblyName]}{ConverterConstant.AbstractionInterface}");
            builder.Append(this.Factory.GetConverter(value.GetType(), this.Index, true).Serialize(value, abstractionInterfaceDictionary));
            return builder.ToString();
        }
    }
}
