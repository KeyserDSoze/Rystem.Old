using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public interface IChart
    {
        string X { get; }
        string Y { get; }
        string Title { get; }
        Dictionary<string, DataModel> Datasets { get; }
    }
}
