using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Utility
{
    public static class Primitive
    {
        public static bool Is(Type type)
        {
            if (type == typeof(string))
                return true;
            return (type.IsValueType & type.IsPrimitive);
        }
    }
}