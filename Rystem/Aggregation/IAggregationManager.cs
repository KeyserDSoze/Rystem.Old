using Microsoft.Extensions.Logging;
using Rystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Aggregation
{
    internal interface IAggregationManager<T> : IAggregationManager, IRystemManager<T>
    {
        Task<IList<T>> RunAsync(IEnumerable<T> events, ILogger log, Func<T, Task> action = null, Func<Exception, T, Task> errorCatcher = null, Installation installation = Installation.Default);
        Task<IList<T>> FlushAsync(ILogger log, Installation installation);
    }
    internal interface IAggregationManager
    {
        Task AutoFlushAsync(Installation installation);
        TimeSpan GetAggregationTime(Installation installation);
        IEnumerable<Installation> GetInstallations();
    }
}