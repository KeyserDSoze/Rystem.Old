using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public interface IFileData : IAggregatedData
    {
        Stream Stream { get; set; }
    }
}
