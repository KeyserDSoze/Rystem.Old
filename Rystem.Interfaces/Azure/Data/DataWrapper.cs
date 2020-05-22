using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Data
{
    public sealed class DataWrapper
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public IDataProperties Properties { get; set; }
    }
}
