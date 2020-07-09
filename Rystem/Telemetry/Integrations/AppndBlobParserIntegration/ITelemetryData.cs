using Rystem.Data;
using System.Collections.Generic;

namespace Rystem
{
    internal interface ITelemetryData<T> : IData
    {
        IEnumerable<T> Events { get; set; }
    }
}