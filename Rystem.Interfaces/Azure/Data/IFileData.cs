using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Data
{
    public interface IFileData : IData
    {
        Stream Stream { get; set; }
    }
}
