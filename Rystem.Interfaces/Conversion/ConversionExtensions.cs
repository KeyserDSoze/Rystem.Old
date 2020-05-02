using Newtonsoft.Json;
using Rystem.Const;

namespace System
{
    public static class JsonConversionExtensions
    {
        public static string ToStandardJson<T>(this T entity)
            => JsonConvert.SerializeObject(entity, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
        public static T FromStandardJson<T>(this string entry)
            => JsonConvert.DeserializeObject<T>(entry, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
    }
}
