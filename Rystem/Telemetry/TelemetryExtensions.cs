using Azure.Storage;
using Microsoft.OData.Edm;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    public static class TelemetryExtensions
    {
        private static IRystemManager<Telemetry> GetTelemetryManager(Telemetry entity)
            => new TelemetryManager(entity.GetConfigurationBuilder(), entity);
        private static ITelemetryManager Manager(this Telemetry entity)
            => entity.DefaultManager<Telemetry>(GetTelemetryManager) as ITelemetryManager;

        public static void Track<TTelemetry>(this TTelemetry telemetry, ITelemetryEvent telemetryEvent, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
        {
            if (telemetryEvent.Timestamp == default)
                telemetryEvent.Timestamp = DateTime.UtcNow;
            telemetryEvent.Telemetry = telemetry;
            telemetry.Events.Add(telemetryEvent);
        }
        public static void TrackMetric<TTelemetry>(this TTelemetry telemetry, string name, object value, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
            => telemetry.Track(new MetricTelemetry { Name = name, Value = value.ToString() }, installation);
        public static void TrackTrace<TTelemetry>(this TTelemetry telemetry, Microsoft.Extensions.Logging.LogLevel logLevel, string message, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
            => telemetry.Track(new TraceTelemetry { LogLevel = logLevel, Message = message }, installation);
        public static void TrackException<TTelemetry>(this TTelemetry telemetry, Exception exception, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
            => telemetry.Track(new ExceptionTelemetry { Exception = exception }, installation);
        public static DependencyTelemetry TrackDependency<TTelemetry>(this TTelemetry telemetry,
            string name,
            Installation installation = Installation.Default,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string pathCaller = "",
            [CallerLineNumber] int lineNumberCaller = 0)
            where TTelemetry : Telemetry
            => new DependencyTelemetry
            {
                Name = name,
                Caller = caller,
                PathCaller = pathCaller,
                LineNumberCaller = lineNumberCaller,
                StopAction = (dependency) => telemetry.Track(dependency, installation),
                Telemetry = telemetry
            };

        public static async Task StopAsync<TTelemetry>(this TTelemetry telemetry, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
        {
            telemetry.End = DateTime.UtcNow;
            await telemetry.Manager().TrackEventAsync(telemetry, installation).NoContext();
        }
        public static void Stop<TTelemetry>(this TTelemetry telemetry, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
            => telemetry.StopAsync().ToResult();
        public static async Task<IEnumerable<Telemetry>> GetEventsAsync<TTelemetry>(this TTelemetry telemetry, DateTime from, DateTime to, string key = null, Installation installation = Installation.Default)
            where TTelemetry : Telemetry
            => await telemetry.Manager().GetEventsAsync(from, to, key, installation).NoContext();
    }
}