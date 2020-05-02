using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Conversion
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class CsvIgnore : Attribute
    {
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class CsvProperty : Attribute
    {
        public string Name { get; }
        public CsvProperty(string name)
            => this.Name = name;
    }
}
