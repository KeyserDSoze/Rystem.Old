using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Rystem.Aggregation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem
{
    public static class SqlParserTest
    {
        private static SqlParser sqlParser;
        public static void SetSqlParser(string connectionString)
        {
            sqlParser = new SqlParser(new TelemetryConfiguration()
            {
                Name = "Combo",
                ConnectionString = connectionString,
                Type = TelemetryType.Sql
            });
        }
        public static async Task Set(IList<Telemetry> events)
        {
            await sqlParser.ParseAsync("sample", events, null, Installation.Default).NoContext();
        }
    }
    internal class SqlParser : IAggregationParser<Telemetry>
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        private readonly string InsertQuery;
        private SqlConnection NewConnection() => new SqlConnection(this.TelemetryConfiguration.ConnectionString);
        public SqlParser(TelemetryConfiguration telemetryConfiguration)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            this.InsertQuery = $"INSERT INTO SmartTelemetry_{telemetryConfiguration.Name} (Id, Start, End, Key, ElapsedTime) VALUES (@Id, @Start, @End, @Key, @ElapsedTime);";
        }
        //private TableName SmartTelemetry => new TableName()
        //{
        //    Name = $"SmartTelemetry_{TelemetryConfiguration.Name}",
        //    Parameters = new List<TableParameter>
        //    {
        //        new TableParameter
        //        {
        //            IsIdentity = false,
        //            IsNullable = false,
        //            IsPrimaryKey = true,
        //            Name = "Id",
        //            Type = TablePrameterType.BigInt
        //        }
        //    }
        //};

        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            StringBuilder query = new StringBuilder();
            StringBuilder eventsQuery = new StringBuilder();
            foreach (Telemetry telemetry in events)
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@Id", telemetry.Id));
                parameters.Add(new SqlParameter("@Start", telemetry.Start));
                parameters.Add(new SqlParameter("@End", telemetry.End));
                parameters.Add(new SqlParameter("@Key", telemetry.Key));
                parameters.Add(new SqlParameter("@ElapsedTime", telemetry.ElapsedTime.Ticks));
                SqlCommand sqlCommand = new SqlCommand(this.InsertQuery);
                sqlCommand.Parameters.Add(parameters);
                query.Append(sqlCommand.CommandText);
                foreach (ITelemetryEvent singleEvent in telemetry.Events)
                {

                }
            }
            using SqlConnection sqlConnection = this.NewConnection();
        }
    }
    internal interface ISqlSaver<T>
        where T : ITelemetryEvent
    {
        Task Save(T singleEvent);
    }
}