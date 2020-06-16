using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Data
{
    public sealed class DataWrapper
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
    }
}
