using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.ZTester
{
    public abstract class ALog : ITableStorage
    {
        public bool Success { get; set; }
        public string Operator { get; set; }
        public string OperatorAction { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }

        public string Service { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Annotation { get; set; } = string.Empty;

        public abstract OperatorAction LogType { get; set; }

        private DateTime tsLocalTime = DateTime.MinValue;
        public DateTime TimeStampLocalTime
        {
            get => Timestamp.CentralEuropeTime();
            set => tsLocalTime = value;
        }
    }
    public static class Other 
    {
        public static DateTime CentralEuropeTime(this DateTime dateTime) => TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
    }
    public class Deactivation : ALog
    {
        static Deactivation()
        {
            //TableStorageInstaller.Configure<Deactivation>(Consts.CCTableStorageConnection, Rystem.Enums.Installation.Default, $"{nameof(Deactivation)}Log");
            NoSqlInstaller.Configure<Deactivation>(
                new NoSqlConfiguration()
                {
                    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=wondacustomercare;AccountKey=evXjvsbmnDahOb9R3TmwJqxEd6KsHPLla3VZQwYxTFPA6AH/LlFok0L4MMlB9bWYVpFJOArNJiveK3V4ZKd/Qg==;EndpointSuffix=core.windows.net",
                    Type = NoSqlType.TableStorage,
                    Name = $"{nameof(Deactivation)}Log"
                },
                Rystem.Enums.Installation.Default);
        }
        public Deactivation() : base()
        {
        }

        public int ServiceID { get; set; }
        public string Platform { get; set; }
        public override OperatorAction LogType { get { return ZTester.OperatorAction.Deactivation; } set { } }
    }
    public enum OperatorAction
    {
        All = 1000,
        Deactivation,
        DeactivationAll,
        NumberReset,
        Search,
        Info,
        Refund,
        RefundInstruction,
        Block
    }
}
