using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rystem.Conversion
{
    internal class PrimitiveConverter : Converter
    {
        public PrimitiveConverter(IConverterFactory factory, int index) : base(factory, index) { }

        internal override dynamic Deserialize(Type type, string value, IDictionary<int, string> antiAbstractionInterfaceDictionary)
        {
            if (value == null)
                return default;
            if (type.BaseType != typeof(Enum))
            {
                return (!string.IsNullOrWhiteSpace(value) ?
                    (!type.IsGenericType ?
                        Convert.ChangeType(value, type, CultureInfo.InvariantCulture) :
                        Convert.ChangeType(value, type.GenericTypeArguments[0], CultureInfo.InvariantCulture)
                    )
                    : default);
            }
            else
            {
                return Enum.Parse(type, value);
            }
        }

        internal override string Serialize(object value, IDictionary<string, int> abstractionInterfaceDictionary)
        {
            return value.ToString();
        }
    }
}
