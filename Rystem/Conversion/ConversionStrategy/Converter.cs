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
        private protected IDictionary<string, string> AbstractionInterfaceMapping;
        internal IDictionary<string, string> GetMapping() => this.AbstractionInterfaceMapping;
        private protected IConverterFactory Factory;
        private protected int Index;
        private protected char IndexAsChar;
        private protected char AbstractionInterface;
        private protected char Enumerable;
        private protected char Dictionarable;
        private protected char ArrayLength;
        public Converter(IConverterFactory factory, int index, IDictionary<string, string> abstractionInterfaceMapping)
        {
            this.Factory = factory;
            this.Index = index;
            this.IndexAsChar = (char)(ConverterConstant.Start - index);
            this.AbstractionInterface = (char)(ConverterConstant.AbstractionInterface - index);
            this.Enumerable = (char)(ConverterConstant.Enumerable - index);
            this.Dictionarable = (char)(ConverterConstant.Dictionarable - index);
            this.ArrayLength = (char)(ConverterConstant.ArrayLength - index);
            this.AbstractionInterfaceMapping = abstractionInterfaceMapping;
        }
        private protected string HelpToSerialize(Type type, object value)
            => this.Factory.GetConverter(type, this.Index, this).Serialize(value);
        private protected dynamic HelpToDeserialize(Type type, string value)
            => this.Factory.GetConverter(type, this.Index, this).Deserialize(type, value);
        internal abstract string Serialize(object value);
        internal abstract dynamic Deserialize(Type type, string value);
    }
}
