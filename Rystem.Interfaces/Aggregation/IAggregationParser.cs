using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Rystem.Aggregation
{
    public interface IAggregationParser
    {
        Task ParseAsync<T>(string queueName, IList<T> events, ILogger log, Installation installation);
    }
}
