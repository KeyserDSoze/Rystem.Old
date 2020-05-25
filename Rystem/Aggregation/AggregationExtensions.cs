using Microsoft.Extensions.Logging;
using Rystem.Azure;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Aggregation
{
    public static class AggregationExtensions
    {
        private static IManager<T> GetAggregationManager<T>(IAggregation<T> entity)
            => new AggregationManager<T>(entity.GetConfigurationBuilder());
        private static IAggregationManager<T> Manager<T>(this IAggregation<T> entity)
            => entity.DefaultManager(GetAggregationManager) as IAggregationManager<T>;
        
        public static async Task<IList<T>> RunAsync<T>(this IAggregation<T> aggregation, IEnumerable<T> events, ILogger log, Func<T, Task> action = null, Func<Exception, T, Task> errorCatcher = null, Installation installation = Installation.Default)
            => await aggregation.Manager().RunAsync(events, log, action, errorCatcher, installation).NoContext();
        public static async Task<IList<T>> FlushAsync<T>(this IAggregation<T> aggregation, ILogger log, Installation installation = Installation.Default)
           => await aggregation.Manager().FlushAsync(log, installation).NoContext();

        public static IList<T> Run<T>(this IAggregation<T> aggregation, IEnumerable<T> events, ILogger log, Action<T> action = null, Action<Exception, T> errorCatcher = null, Installation installation = Installation.Default)
        {
            return aggregation.RunAsync(events, log, WrappedAction, WrappedError, installation).ToResult();

            Task WrappedAction(T t)
            {
                action?.Invoke(t);
                return Task.CompletedTask;
            }
            Task WrappedError(Exception e, T t)
            {
                errorCatcher.Invoke(e, t);
                return Task.CompletedTask;
            }
        }

        public static IList<T> Flush<T>(this IAggregation<T> aggregation, ILogger log, Installation installation = Installation.Default)
            => aggregation.FlushAsync(log, installation).ToResult();
    }
}
