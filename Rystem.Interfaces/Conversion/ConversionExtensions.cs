using Newtonsoft.Json;
using Rystem.Const;
using Rystem.Conversion;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace System
{
    public static class JsonConversionExtensions
    {
        public static string ToDefaultJson<T>(this T entity)
            => JsonConvert.SerializeObject(entity, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
        public static T FromDefaultJson<T>(this string entry)
            => JsonConvert.DeserializeObject<T>(entry, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);

        public static string ToRystemCsv<T>(this T entity)
           => entity.ToObjectCsv();
        public static T FromRystemCsv<T>(this string entry)
            => entry.FromObjectCsv<T>();

        public static string ToCsv<T>(this IEnumerable<T> entities, char splittingChar = ',')
            where T : new()
            => new CsvDefaultConversion<T>(splittingChar).Write(entities);
        public static async Task<IEnumerable<T>> FromCsvAsync<T>(this Stream stream, char splittingChar = ',')
            where T : new()
            => await new CsvDefaultConversion<T>(splittingChar).ReadAsync(stream).NoContext();
        public static IEnumerable<T> FromCsv<T>(this string entry, char splittingChar = ',')
            where T : new()
            => new CsvDefaultConversion<T>(splittingChar).Read(entry);
        public static IEnumerable<T> FromCsv<T>(this IEnumerable<string> entries, char splittingChar = ',')
            where T : new()
            => new CsvDefaultConversion<T>(splittingChar).Read(entries);
    }
}
