using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal interface ITelemetryManager : IRystemManager<Telemetry>
    {
        Task TrackEvent(Telemetry telemetry, Installation installation);
    }
}