using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{

    internal interface IConverterFactory
    {
        Converter GetConverter(Type valueType, int index, Converter caller);
    }
}
