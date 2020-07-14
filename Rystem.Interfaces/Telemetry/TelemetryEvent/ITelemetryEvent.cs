using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public interface ITelemetryEvent
    {
        DateTime Timestamp { get; set; }
    }
    public interface IRystemTelemetryEvent
    {

    }
}