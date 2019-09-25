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
        public static string Serialize<T>(T data)
           where T : new() => new StartConversion().Serialize(data);
        public static T Deserialize<T>(string value)
           where T : new() => new StartConversion().Deserialize(typeof(T), value);
    }
}
