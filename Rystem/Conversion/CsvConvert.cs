using Rystem.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    public static class CsvConvert
    {
        private static IConverterFactory Factory = new SerializerFactory();
        public static string Serialize<T>(T data)
           where T : new() => Factory.GetConverter(data.GetType(), ConverterConstant.Start).StartSerialization(data);
        public static T Deserialize<T>(string value)
           where T : new() => Factory.GetConverter(typeof(T), ConverterConstant.Start).StartDeserialization(typeof(T), value);
    }
}
