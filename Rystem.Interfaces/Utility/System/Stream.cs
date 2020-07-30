using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StreamExtensions
    {
        public static async Task<string> GetValueAsync(this Stream stream)
        {
            using StreamReader streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync().NoContext();
        }
        public static string GetValue(this Stream stream)
        {
            using StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
        public static async Task SetValueAsync(this Stream stream, string value)
        {
            using StreamWriter streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(value);
        }
        public static void SetValue(this Stream stream, string value)
        {
            using StreamWriter streamWriter = new StreamWriter(stream);
            streamWriter.Write(value);
        }
    }
}
