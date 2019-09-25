using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal class ArrayConverter : Converter
    {
        public ArrayConverter(IConverterFactory factory, int index) : base(factory, index) { }

        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            Type elementType = type.GetElementType();
            string[] splitted = value.ToMyIndexSplit(ConverterConstant.ArrayLength);
            Array array = Array.CreateInstance(elementType, int.Parse(splitted[0]));
            string[] values = splitted[1].ToMySplit(ConverterConstant.Enumerable);
            for (int i = 0; i < array.Length; i++)
            {
                if (i >= values.Length)
                    break;
                array.SetValue(this.Factory.GetConverter(elementType, this.Index).Deserialize(elementType, values[i], antiAbstractionInterfaceDictionary), i);
            }
            return array;
        }

        internal override string Serialize(object value, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            return $"{(value as Array).Length}{ConverterConstant.ArrayLength}{new EnumerableConverter(this.Factory, this.Index).Serialize(value, abstractionInterfaceDictionary)}";
        }
    }
}
