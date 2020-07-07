using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public abstract class Telemetry : IConfigurator
    {
        //todo: telemetry mettere gli alert impostabili in configuration builder
        public string Id { get; set; } = Alea.GetTimedKey();
        public string Key { get; set; }
        public DateTime Start { get; set; } = DateTime.UtcNow;
        public DateTime End { get; set; }
        public TimeSpan Duration => End.Subtract(Start);
        public List<ITelemetryEvent> Events { get; } = new List<ITelemetryEvent>();
        public abstract ConfigurationBuilder GetConfigurationBuilder();
        public static T CreateNew<T>() where T : Telemetry, new()
            => new T();
        public static T CreateNew<T>(string key) where T : Telemetry, new()
        {
            T telemetry = new T();
            telemetry.Key = key;
            return telemetry;
        }
    }
}