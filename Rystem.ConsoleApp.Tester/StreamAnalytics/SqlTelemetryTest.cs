using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
using Rystem.UnitTest;
using Rystem.Utility.SqlReflection;
using Rystem.ZConsoleApp.Tester.DummyHttpContextAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.StreamAnalytics
{
    public class SqlTelemetryTest : IUnitTest
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
                myTelemetry.TrackMetric("solid", 550);
                myTelemetry.TrackTrace(Microsoft.Extensions.Logging.LogLevel.Information, "Good is good");
                myTelemetry.Track(new MyCustomEvent { Saturday2 = 44, Value = "aaaaaa", Fortunes = new List<Fortune> { new Fortune { Molecule = 3 }, new Fortune { Molecule = i } } });
                myTelemetry.TrackRequest(MyHttpContext.SimulateRequest(new byte[0], "text/plain", System.Net.IPAddress.Parse("125.25.26.23"), 45, System.Net.IPAddress.Parse("125.25.26.23")));
                await Task.Delay(100);
                dependency.Stop();
                await myTelemetry.StopAsync();
            }
            //IEnumerable<Telemetry> telemetries = await Telemetry.CreateNew<MyTelemetry>()
            //    .GetEventsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)).NoContext();
            //int value = telemetries.Count();
            //metrics.AddOk(value, "Count");
        }
        public class MyCustomEvent : ITelemetryEvent
        {
            public DateTime Timestamp { get; set; }
            public string Value { get; set; }
            public long Saturday2 { get; set; }
            public List<Fortune> Fortunes { get; set; }
            public Fortune Fortune { get; set; } = new Fortune { Molecule = 56 };
            public Telemetry Telemetry { get; set; }
            public string Id { get; set; }
            public string TelemetryId { get; set; }
        }
        public class Fortune
        {
            public int Molecule { get; set; }
        }

        public class MyTelemetry : Telemetry
        {
            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithTelemetry(KeyManager.Instance.Sql)
                    .WithSql(new SqlTelemetryBuilder("Paolone", 50, TimeSpan.FromMinutes(1)))
                    .AddCustomEvent<MyCustomEvent>(new SqlTable("Soros")
                        .With("Value", SqlTablePrameterType.VarChar(100))
                            .Compose<MyCustomEvent>(x => x.Value)
                        .With("Fortunes", SqlTablePrameterType.VarChar(8000))
                            .Compose<MyCustomEvent>(x => x.Fortunes.ToDefaultJson()))
                    .Build();
            }
        }
    }
}