using Rystem.Interfaces.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    public static class CsvConvertV2
    {
        private static IConverterFactory Factory = new ConverterFactory();
        public static string Serialize<T>(T data)
           where T : new() => Factory.GetConverter(data.GetType(), 2600).Convert(data);
    }
}
