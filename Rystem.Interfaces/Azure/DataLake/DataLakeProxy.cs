using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public sealed class DataLakeDummy
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public LakeProperties Properties { get; set; }
    }
}
