using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Conversion
{
    internal static class ConverterExtension
    {
        internal static string[] ToMySplit(this string value, int x)
           => value.Split((char)x);
        internal static string[] ToMyIndexSplit(this string value, int x)
        {
            char splittingBy = (char)x;
            string splittingByAsString = splittingBy.ToString();
            string[] splitted = value.Split(splittingBy);
            return new string[2] {
                splitted[0],
                string.Join(splittingByAsString, splitted.Skip(1))
            };
        }
        internal static string ToMyTrim(this string value, int x)
            => value.Trim((char)x);
    }
}
