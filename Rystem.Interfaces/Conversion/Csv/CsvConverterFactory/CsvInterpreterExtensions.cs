using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal static class CsvInterpreterExtensions
    {
        public static ICsvInterpreter CreateConverter(this ICsvInterpreter interpreter, Type objectType, int index, IDictionary<string, string> abstractionInterfaceMapping)
        {
            if (StringablePrimitive.CheckWithNull(objectType))
                return new PrimitiveConverter(index + 1, abstractionInterfaceMapping);
            else if (!(interpreter is AbstractInterfaceConverter) && (objectType.IsAbstract || objectType.IsInterface || objectType.GetInterfaces().Length > 0 || objectType.BaseType.IsAbstract))
                return new AbstractInterfaceConverter(index, abstractionInterfaceMapping);
            else if (objectType.IsArray)
                return new ArrayConverter(index + 1, abstractionInterfaceMapping);
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
                return new EnumerableConverter( index + 1, abstractionInterfaceMapping);
            else
                return new ObjectConverter(index + 1, abstractionInterfaceMapping);
        }
    }
}
