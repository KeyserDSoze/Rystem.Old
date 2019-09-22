using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public interface IDataLakeReader
    {
        dynamic Read(Stream stream);
    }
}
