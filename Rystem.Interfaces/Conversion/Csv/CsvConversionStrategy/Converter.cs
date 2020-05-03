using System;
using System.Collections.Generic;

namespace Rystem.Conversion
{
    internal abstract class Converter
    {
        private protected IDictionary<string, string> AbstractionInterfaceMapping = new Dictionary<string, string>();
        private protected int Index;
        private protected char IndexAsChar;
        private protected char AbstractionInterface;
        private protected char Enumerable;
        private protected char Dictionarable;
        private protected char ArrayLength;
        public Converter(int index, IDictionary<string, string> abstractionInterfaceMapping)
        {
            this.Index = index;
            this.IndexAsChar = (char)(ConverterConstant.Start - index);
            this.AbstractionInterface = (char)(ConverterConstant.AbstractionInterface - index);
            this.Enumerable = (char)(ConverterConstant.Enumerable - index);
            this.Dictionarable = (char)(ConverterConstant.Dictionarable - index);
            this.ArrayLength = (char)(ConverterConstant.ArrayLength - index);
            this.AbstractionInterfaceMapping = abstractionInterfaceMapping;
        }
        private protected string HelpToSerialize(Type type, object value)
            => (this as ICsvInterpreter).CreateConverter(type, this.Index, this.AbstractionInterfaceMapping).Serialize(value);
        private protected dynamic HelpToDeserialize(Type type, string value)
            => (this as ICsvInterpreter).CreateConverter(type, this.Index, this.AbstractionInterfaceMapping).Deserialize(type, value);
    }
}
