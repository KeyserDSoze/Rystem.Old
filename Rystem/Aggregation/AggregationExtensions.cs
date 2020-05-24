using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Aggregation
{
    public static class AggregationExtensions
    {
        private readonly static Dictionary<string, IAggregationManager> Managers = new Dictionary<string, IAggregationManager>();
        private readonly static object TrafficLight = new object();
        private static AggregationManager<T> Manager<T>(this IAggregation entity)
        {
            Type entityType = typeof(T);
            if (!Managers.ContainsKey(entityType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(entityType.FullName))
                    {
                        Type genericType = typeof(AggregationManager<>).MakeGenericType(entityType);
                        Managers.Add(entityType.FullName, (IAggregationManager)Activator.CreateInstance(genericType, entity.GetConfigurationBuilder()));
                    }
            return Managers[entityType.FullName] as AggregationManager<T>;
        }
        public static async Task<IList<T>> RunAsync<T>(this IAggregation aggregation, IEnumerable<T> events, ILogger log, Func<T, Task> action = null, Func<Exception, T, Task> errorCatcher = null, Installation installation = Installation.Default)
            => await aggregation.Manager<T>().RunAsync(events, log, action, errorCatcher, installation).NoContext();
        public static async Task<IList<T>> FlushAsync<T>(this IAggregation aggregation, ILogger log, Installation installation = Installation.Default)
           => await aggregation.Manager<T>().FlushAsync(log, installation).NoContext();

        public static IList<T> Run<T>(this IAggregation aggregation, IEnumerable<T> events, ILogger log, Action<T> action = null, Action<Exception, T> errorCatcher = null, Installation installation = Installation.Default)
        {
            return aggregation.Manager<T>().RunAsync(events, log, WrappedAction, WrappedError, installation).ToResult();

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

        public static IList<T> Flush<T>(this IAggregation aggregation, ILogger log, Installation installation = Installation.Default)
            => aggregation.FlushAsync<T>(log, installation).ToResult();
    }
}
