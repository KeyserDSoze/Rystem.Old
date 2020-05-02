using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class Conversion
    {
        public static string ToStandardJson<T>(this T entity)
            where T : new()
            => JsonConvert.SerializeObject(entity, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
        public static T FromStandardJson<T>(this string entry)
            where T : new()
            => JsonConvert.DeserializeObject<T>(entry, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
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
