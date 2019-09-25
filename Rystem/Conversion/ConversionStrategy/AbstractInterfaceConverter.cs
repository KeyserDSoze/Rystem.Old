using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    internal class AbstractInterfaceConverter : Converter
    {
        public AbstractInterfaceConverter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping) : base(factory, index, abstractionInterfaceMapping) { }
        internal override dynamic Deserialize(Type type, string value)
        {
            string[] values = value.Split(this.AbstractionInterface);
            return this.HelpToDeserialize(this.GetAbstractionInterfaceImplementation(values[0]), values[1]);
        }

        internal override string Serialize(object value)
            => $"{this.SetAbstractionInterface(value.GetType())}{(this.AbstractionInterface)}{this.HelpToSerialize(value.GetType(), value)}";
        private string SetAbstractionInterface(Type type)
        {
            string assemblyName = type.AssemblyQualifiedName;
            if (!this.AbstractionInterfaceMapping.ContainsKey(assemblyName))
                this.AbstractionInterfaceMapping.Add(assemblyName, this.AbstractionInterfaceMapping.Count.ToString());
            return this.AbstractionInterfaceMapping[assemblyName];
        }
        private Type GetAbstractionInterfaceImplementation(string value)
        {
            return Type.GetType(this.AbstractionInterfaceMapping[value]);
        }
    }
}
