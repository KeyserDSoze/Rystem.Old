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
    }
}
