using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public interface IData
    {
        string Name { get; set; }
        IDataProperties Properties { get; set; }
    }
}
