using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    public class SerializerFactory : IConverterFactory
    {
        public Converter GetConverter(Type objectType, int index, bool comeFromAbstract = false)
        {
            if (StringablePrimitive.CheckWithNull(objectType))
                return new PrimitiveConverter(this, index - 1);
            else if (!comeFromAbstract && (objectType.IsAbstract || objectType.IsInterface || objectType.GetInterfaces().Length > 0 || objectType.BaseType.IsAbstract))
                return new AbstractInterfaceConverter(this, index);
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
                return new EnumerableConverter(this, index - 1);
            else
                return new ObjectConverter(this, index - 1);
        }
    }
}
