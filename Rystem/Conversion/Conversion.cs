using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class CsvConversionExtesions
    {
        public static string ToStandardCsv<T>(this T entity)
            where T : new()
            => entity.ToCsv();
        public static T FromStandardCsv<T>(this string entry)
            where T : new()
            => entry.FromCsv<T>();
        //public static string ToCsv<T>(this T entity) { }
        //public static string FromCsv<T>(this T entity) { }
    }
}
