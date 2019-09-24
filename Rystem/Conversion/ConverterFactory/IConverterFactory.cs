using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{

    public interface IConverterFactory
    {
        Converter GetConverter(Type valueType, int index, bool comeFromAbstract = false);
    }
}
