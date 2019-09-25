using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    internal class SerializerFactory : IConverterFactory
    {
        public Converter GetConverter(Type objectType, int index, Converter caller)
        {
            if (StringablePrimitive.CheckWithNull(objectType))
                return new PrimitiveConverter(this, index + 1, caller.GetMapping());
            else if (!(caller is AbstractInterfaceConverter) && (objectType.IsAbstract || objectType.IsInterface || objectType.GetInterfaces().Length > 0 || objectType.BaseType.IsAbstract))
                return new AbstractInterfaceConverter(this, index, caller.GetMapping());
            else if (objectType.IsArray)
                return new ArrayConverter(this, index + 1, caller.GetMapping());
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
                return new EnumerableConverter(this, index + 1, caller.GetMapping());
            else
                return new ObjectConverter(this, index + 1, caller.GetMapping());
        }
    }
}
