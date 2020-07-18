using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal interface ITelemetryManager : IRystemManager<Telemetry>
    {
        Task TrackEventAsync(Telemetry telemetry, Installation installation);
        Task<IEnumerable<Telemetry>> GetEventsAsync(Expression<Func<Telemetry, bool>> expression, Installation installation);
    }
}