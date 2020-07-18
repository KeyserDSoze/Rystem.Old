using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    public interface ITelemetryIntegration
    {
        Task<IEnumerable<Telemetry>> GetEventsAsync(DateTime from, DateTime to, string key);
    }
}