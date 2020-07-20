using Microsoft.AspNetCore.Http;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.DummyHttpContextAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.StreamAnalytics
{
    public class TelemetryTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            //for (int i = 0; i < 120; i++)
            //{
            //    var myTelemetry = Telemetry.CreateNew<MyTelemetry>(i <= 60 ? (i % 2).ToString() : null);
            //    var dependency = myTelemetry.TrackDependency("call to something");
            //    try
            //    {
            //        throw new NotImplementedException("Big error");
            //    }
            //    catch (Exception er)
            //    {
            //        myTelemetry.TrackException(er);
            //    }
            //    myTelemetry.TrackMetric(new MetricTelemetry { Name = "solid", Value = 550.ToString() });
            //    myTelemetry.TrackTrace(new TraceTelemetry { LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Good is good" });
            //    myTelemetry.Track(new MyCustomEvent { Saturday = 44, Value = "aaaaaa" });
            //    myTelemetry.TrackRequest(MyHttpContext.SimulateRequest(new byte[0], "text/plain", System.Net.IPAddress.Parse("125.25.26.23"), 45, System.Net.IPAddress.Parse("125.25.26.23")));
            //    await Task.Delay(100);
            //    dependency.Stop();
            //    await myTelemetry.StopAsync();
            //}
            IEnumerable<Telemetry> telemetries = await Telemetry.CreateNew<MyTelemetry>()
                .GetEventsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)).NoContext();
            int value = telemetries.Count();
            metrics.AddOk(value, "Count");
        }
        public class MyCustomEvent : ITelemetryEvent
        {
            public DateTime Timestamp { get; set; }
            public string Value { get; set; }
            public long Saturday { get; set; }
            [JsonIgnore]
            public Telemetry Telemetry { get; set; }
            public string Id { get; set; }
            public string TelemetryId { get; set; }
        }

        public class MyTelemetry : Telemetry
        {
            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithTelemetry(KeyManager.Instance.Storage)
                    .WithAppendBlob(new AppendBlobTelemetryBuilder("supertelemetry", 50, TimeSpan.FromMinutes(1)))
                    .Build();
            }
        }
    }
}