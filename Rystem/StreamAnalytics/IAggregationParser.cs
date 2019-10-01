using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Rystem.Enums;

namespace Rystem.StreamAnalytics
{
    public interface IAggregationParser
    {
        void Parse<T>(string queueName, IList<T> events, ILogger log, Installation installation);
    }
}
