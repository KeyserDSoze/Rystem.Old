using Microsoft.AspNetCore.Http;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.StreamAnalytics
{
    public class TelemetryTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            for (int i = 0; i < 120; i++)
            {
                var myTelemetry = Telemetry.CreateNew<MyTelemetry>(i <= 60 ? (i % 2).ToString() : null);
                var dependency = myTelemetry.TrackDependency("call to something");
                try
                {
                    throw new NotImplementedException("Big error");
                }
                catch (Exception er)
                {
                    myTelemetry.TrackException(er);
                }
                myTelemetry.TrackMetric(new MetricTelemetry { Name = "solid", Value = 550.ToString() });
                myTelemetry.TrackTrace(new TraceTelemetry { LogLevel = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Good is good" });
                myTelemetry.Track(new MyCustomEvent { Saturday = 44, Value = "aaaaaa" });
                await Task.Delay(100);
                dependency.Stop();
                await myTelemetry.StopAsync();
            }
        }
        public class MyCustomEvent : ITelemetryEvent
        {
            public DateTime Timestamp { get; set; }
            public string Value { get; set; }
            public long Saturday { get; set; }
        }

        public class MyTelemetry : WebTelemetry
        {
            public MyTelemetry() : base(null) { }
            public MyTelemetry(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
            {
            }
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
