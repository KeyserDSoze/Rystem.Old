using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal class ArrayConverter : Converter
    {
        public ArrayConverter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping) : base(factory, index, abstractionInterfaceMapping) { }

        internal override dynamic Deserialize(Type type, string value)
        {
            Type elementType = type.GetElementType();
            string[] splitted = value.Split(this.ArrayLength);
            Array array = Array.CreateInstance(elementType, int.Parse(splitted[0]));
            string[] values = splitted[1].Split(this.Enumerable);
            for (int i = 0; i < array.Length; i++)
            {
                if (i >= values.Length)
                    break;
                array.SetValue(this.HelpToDeserialize(elementType, values[i]), i);
            }
            return array;
        }

        internal override string Serialize(object value)
        {
            return $"{(value as Array).Length}{this.ArrayLength}{new EnumerableConverter(this.Factory, this.Index, this.AbstractionInterfaceMapping).Serialize(value)}";
        }
    }
}
