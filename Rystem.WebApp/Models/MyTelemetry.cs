using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rystem.WebApp.Models
{
    public class MyTelemetry : Telemetry
    {
        public override ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                    .WithTelemetry("Server=tcp:kynsextesting.database.windows.net,1433;Initial Catalog=Testing;Persist Security Info=False;User ID=kynsex;Password=Delorean2020;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
                    .WithSql(new SqlTelemetryBuilder("WebRystem", 500, TimeSpan.FromMinutes(5)))
                    .Build();
        }
    }
}
